namespace SevenSigils.Api.Contracts.Auth;

public sealed record AuthResponse(
    string AccessToken,
    string Email,
    IReadOnlyList<string> Roles);