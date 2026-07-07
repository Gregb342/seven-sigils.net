using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;
using SevenSigils.Infrastructure.Options;

namespace SevenSigils.Infrastructure.Seeding;

/// <summary>
/// Crée le compte admin au démarrage à partir de la configuration (Admin:Email / Admin:Password).
/// Aucune valeur par défaut : sans configuration, aucun compte n'est créé.
/// Idempotent : un compte existant n'est jamais modifié (un mot de passe changé
/// depuis le seed initial ne doit pas être écrasé à chaque redémarrage).
/// </summary>
public sealed class AdminUserSeeder
{
    // Un back-office expose des écritures : on refuse de seeder un admin au mot de passe faible.
    private const int MinPasswordLength = 12;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AdminSeedOptions _options;
    private readonly ILogger<AdminUserSeeder> _logger;

    public AdminUserSeeder(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IOptions<AdminSeedOptions> options,
        ILogger<AdminUserSeeder> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var hasEmail = !string.IsNullOrWhiteSpace(_options.Email);
        var hasPassword = !string.IsNullOrWhiteSpace(_options.Password);

        if (!hasEmail && !hasPassword)
        {
            _logger.LogWarning(
                "No admin account seeded: Admin:Email and Admin:Password are not configured. " +
                "The back-office will be unreachable until an admin exists.");
            return;
        }

        // Configuration partielle = erreur de déploiement : on échoue bruyamment
        // plutôt que de laisser croire qu'un admin a été créé.
        if (!hasEmail || !hasPassword)
        {
            throw new InvalidOperationException(
                "Admin seeding is misconfigured: Admin:Email and Admin:Password must both be set (or both left empty).");
        }

        if (!_options.Email!.Contains('@'))
        {
            throw new InvalidOperationException("Admin:Email is not a valid email address.");
        }

        if (_options.Password!.Length < MinPasswordLength)
        {
            throw new InvalidOperationException(
                $"Admin:Password must be at least {MinPasswordLength} characters long.");
        }

        var normalizedEmail = _options.Email.Trim().ToLowerInvariant();
        var existing = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existing is not null)
        {
            _logger.LogInformation("Admin account '{Email}' already exists — skipping seed.", normalizedEmail);
            return;
        }

        var admin = new ApplicationUser(
            Id: Guid.NewGuid().ToString("N"),
            Email: normalizedEmail,
            PasswordHash: _passwordHasher.Hash(_options.Password),
            Roles: ["Admin"]);

        await _userRepository.CreateAsync(admin, cancellationToken);

        _logger.LogInformation("Seeded admin account '{Email}'.", normalizedEmail);
    }
}
