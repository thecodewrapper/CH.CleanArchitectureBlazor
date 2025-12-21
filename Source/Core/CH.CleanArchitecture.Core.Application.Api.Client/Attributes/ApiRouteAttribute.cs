namespace CH.CleanArchitecture.Core.Application.Api;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ApiRouteAttribute : Attribute
{
    public ApiRouteAttribute(string template, string method) {
        Template = template.StartsWith("/") ? template : "/" + template;
        Method = new HttpMethod(method.ToUpperInvariant());
    }

    public string Template { get; }
    public HttpMethod Method { get; }
    public string? Name { get; init; }
}
