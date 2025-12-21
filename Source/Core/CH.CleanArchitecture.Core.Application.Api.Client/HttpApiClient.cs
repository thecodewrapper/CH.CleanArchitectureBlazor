using System.Reflection;
using System.Text;
using System.Text.Json;

namespace CH.CleanArchitecture.Core.Application.Api;

public sealed class HttpApiClient : IApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json;

    public HttpApiClient(HttpClient http, JsonSerializerOptions? json = null) {
        _http = http;
        _json = json ?? new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ApiResponse> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : class, IApiRequest {
        var response = await SendAsync<Unit, TRequest>(request, cancellationToken);

        return new ApiResponse(
            new ApiResponsePayload
            {
                Status = response.Payload.Status,
                Message = response.Payload.Message,
                Errors = response.Payload.Errors
            },
            response.StatusCode);
    }

    public async Task<ApiResponse<TResponse>> SendAsync<TResponse, TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : class, IApiRequest {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var route = ResolveRoute<TRequest>();
        var usedRouteKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var path = ApplyRouteTokens(route.Template, request!, usedRouteKeys);

        if (route.Method == HttpMethod.Get) {
            path = AppendQueryString(path, request!, usedRouteKeys);
        }

        using var msg = new HttpRequestMessage(route.Method, path);

        if (route.Method != HttpMethod.Get && route.Method != HttpMethod.Delete) {
            msg.Content = new StringContent(
                JsonSerializer.Serialize(request, _json),
                Encoding.UTF8,
                "application/json");
        }

        using var resp = await _http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        var statusCode = (int)resp.StatusCode;

        // IMPORTANT: read body once
        var content = await resp.Content.ReadAsStringAsync(cancellationToken);

        // 1) Try envelope first (both success and failure)
        var envelope = TryDeserialize<ApiResponsePayload<TResponse>>(content, _json);
        if (envelope is not null) {
            return new ApiResponse<TResponse>(envelope, statusCode);
        }

        // 2) If not envelope and HTTP is failure -> return generic failure payload
        if (!resp.IsSuccessStatusCode) {
            return new ApiResponse<TResponse>(
                new ApiResponsePayload<TResponse>
                {
                    Status = false,
                    Message = $"HTTP {statusCode} {resp.ReasonPhrase}",
                    Errors = new List<ApiErrorDto>
                    {
                        new ApiErrorDto { Code = "http_error", Error = Truncate(content) }
                    }
                },
                statusCode);
        }

        // 3) Success HTTP but raw body (not envelope): treat as TResponse directly
        var raw = TryDeserialize<TResponse>(content, _json);
        if (raw is null) {
            return new ApiResponse<TResponse>(
                new ApiResponsePayload<TResponse>
                {
                    Status = false,
                    Message = "Could not deserialize response as ApiResponsePayload<T> or as the expected response type.",
                    Errors = new List<ApiErrorDto>
                    {
                        new ApiErrorDto { Code = "deserialize_failed", Error = Truncate(content) }
                    }
                },
                statusCode);
        }

        return new ApiResponse<TResponse>(
            new ApiResponsePayload<TResponse>
            {
                Status = true,
                Message = "Success",
                Data = raw
            },
            statusCode);
    }

    public Task<ApiResponse<TResponse>> GetAsync<TResponse, TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : class, IApiRequest
        => SendAsync<TResponse, TRequest>(request, cancellationToken);

    public async Task<ApiBinaryResponse> GetBinaryAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : class, IApiRequest {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var route = ResolveRoute<TRequest>();
        if (route.Method != HttpMethod.Get)
            throw new InvalidOperationException($"GetBinaryAsync only supports GET routes. '{typeof(TRequest).Name}' is configured as {route.Method}.");

        var usedRouteKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var path = ApplyRouteTokens(route.Template, request!, usedRouteKeys);
        path = AppendQueryString(path, request!, usedRouteKeys);

        using var msg = new HttpRequestMessage(HttpMethod.Get, path);
        using var resp = await _http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!resp.IsSuccessStatusCode) {
            var content = await resp.Content.ReadAsStringAsync(cancellationToken);

            // best-effort: parse your error envelope
            var err = TryDeserialize<ApiResponsePayload<object>>(content, _json);
            var message = err?.Message ?? $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}";
            throw new HttpRequestException($"{message}. Body: {Truncate(content)}");
        }

        var bytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken);

        var contentType = resp.Content.Headers.ContentType?.MediaType;
        var contentLength = resp.Content.Headers.ContentLength;

        string? fileName = null;
        if (resp.Content.Headers.ContentDisposition is not null) {
            fileName = resp.Content.Headers.ContentDisposition.FileNameStar
                       ?? resp.Content.Headers.ContentDisposition.FileName;
            fileName = fileName?.Trim('"');
        }

        return new ApiBinaryResponse(bytes, contentType, fileName, contentLength);
    }

    private static ApiRouteAttribute ResolveRoute<TRequest>() {
        var attrs = typeof(TRequest).GetCustomAttributes<ApiRouteAttribute>(inherit: false).ToArray();
        if (attrs.Length == 0)
            throw new InvalidOperationException($"Missing [ApiRoute] on request type '{typeof(TRequest).FullName}'.");

        return attrs[0];
    }

    private static string ApplyRouteTokens<TRequest>(string template, TRequest request, ISet<string> usedKeys) {
        var props = typeof(TRequest).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        string result = template;

        foreach (var token in ExtractTokens(template)) {
            var prop = props.FirstOrDefault(p => string.Equals(p.Name, token.Name, StringComparison.OrdinalIgnoreCase));
            if (prop is null)
                throw new InvalidOperationException($"Route token '{token.Raw}' could not be mapped to any public property on '{typeof(TRequest).Name}'.");

            var value = prop.GetValue(request);
            if (value is null)
                throw new InvalidOperationException($"Route token '{token.Raw}' mapped to '{prop.Name}', but its value was null.");

            usedKeys.Add(prop.Name);

            result = result.Replace(token.Raw, Uri.EscapeDataString(Convert.ToString(value)!),
                StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private static string AppendQueryString<TRequest>(string path, TRequest request, ISet<string> usedKeys) {
        var props = typeof(TRequest).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var pairs = new List<string>();

        foreach (var p in props) {
            if (usedKeys.Contains(p.Name)) continue;
            if (!IsSimpleQueryType(p.PropertyType)) continue;

            var value = p.GetValue(request);
            if (value is null) continue;

            var str = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(str)) continue;

            pairs.Add($"{Uri.EscapeDataString(p.Name)}={Uri.EscapeDataString(str)}");
        }

        if (pairs.Count == 0) return path;

        return path.Contains('?')
            ? path + "&" + string.Join("&", pairs)
            : path + "?" + string.Join("&", pairs);
    }

    private static bool IsSimpleQueryType(Type t) {
        t = Nullable.GetUnderlyingType(t) ?? t;

        return t.IsPrimitive
               || t.IsEnum
               || t == typeof(string)
               || t == typeof(Guid)
               || t == typeof(DateTime)
               || t == typeof(DateTimeOffset)
               || t == typeof(TimeSpan)
               || t == typeof(decimal);
    }

    private static T? TryDeserialize<T>(string json, JsonSerializerOptions options) {
        try {
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch {
            return default;
        }
    }

    private static string Truncate(string s, int max = 500)
        => s.Length <= max ? s : s[..max] + "…";

    private readonly record struct Token(string Raw, string Name);

    private static IEnumerable<Token> ExtractTokens(string template) {
        for (int i = 0; i < template.Length; i++) {
            if (template[i] != '{') continue;

            var end = template.IndexOf('}', i + 1);
            if (end < 0) yield break;

            var raw = template.Substring(i, end - i + 1);
            var inner = raw.Substring(1, raw.Length - 2);

            var colon = inner.IndexOf(':');
            var name = colon >= 0 ? inner.Substring(0, colon) : inner;

            yield return new Token(raw, name);
            i = end;
        }
    }
}