using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Auth;

public sealed class AuthService : IAuthService
{
    private static readonly string[] DefaultRoles = ["User"];

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAccessTokenGenerator _accessTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAccessTokenGenerator accessTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _accessTokenGenerator = accessTokenGenerator;
    }

    public async Task<AuthResult> RegisterAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            throw new DuplicateEmailException(normalizedEmail);
        }

        var user = new ApplicationUser(
            Id: Guid.NewGuid().ToString("N"),
            Email: normalizedEmail,
            PasswordHash: _passwordHasher.Hash(command.Password),
            Roles: DefaultRoles);

        await _userRepository.CreateAsync(user, cancellationToken);

        return ToAuthResult(user);
    }

    public async Task<AuthResult> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(command.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        return ToAuthResult(user);
    }

    private AuthResult ToAuthResult(ApplicationUser user) => new(
        AccessToken: _accessTokenGenerator.Generate(user),
        Email: user.Email,
        Roles: user.Roles);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}