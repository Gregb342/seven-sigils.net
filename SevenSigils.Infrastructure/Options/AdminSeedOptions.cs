namespace SevenSigils.Infrastructure.Options;

public sealed class AdminSeedOptions
{
    public const string SectionName = "Admin";

    /// <summary>
    /// false = back-office absent : les routes /api/v1/auth et /api/v1/admin ne
    /// sont pas exposées et aucun compte admin n'est créé (production, où les
    /// données viennent du dépôt). true par défaut pour le développement local.
    /// </summary>
    public bool Enabled { get; set; } = true;

    public string? Email { get; set; }
    public string? Password { get; set; }
}
