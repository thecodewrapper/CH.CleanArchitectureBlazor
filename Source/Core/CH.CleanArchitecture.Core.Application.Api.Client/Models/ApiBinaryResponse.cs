namespace CH.CleanArchitecture.Core.Application.Api
{
    public sealed record ApiBinaryResponse(
    byte[] Bytes,
    string? ContentType,
    string? FileName,
    long? ContentLength
);
}
