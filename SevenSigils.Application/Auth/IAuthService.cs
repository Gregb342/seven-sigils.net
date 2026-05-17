namespace SevenSigils.Application.Auth;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default);
}