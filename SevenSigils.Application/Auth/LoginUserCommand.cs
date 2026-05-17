namespace SevenSigils.Application.Auth;

public sealed record LoginUserCommand(
    string Email,
    string Password);