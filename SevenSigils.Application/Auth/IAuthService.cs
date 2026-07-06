namespace SevenSigils.Application.Auth;

// Pas d'inscription publique : les comptes (admin) sont seedés côté serveur.
public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginUserCommand command, CancellationToken cancellationToken = default);
}