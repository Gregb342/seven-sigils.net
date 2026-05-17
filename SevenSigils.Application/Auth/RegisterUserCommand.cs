namespace SevenSigils.Application.Auth;

public sealed record RegisterUserCommand(
    string Email,
    string Password);