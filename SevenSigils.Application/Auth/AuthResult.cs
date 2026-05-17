namespace SevenSigils.Application.Auth;

public sealed record AuthResult(
    string AccessToken,
    string Email,
    IReadOnlyList<string> Roles);