namespace SevenSigils.Infrastructure.Options;

public sealed class AdminSeedOptions
{
    public const string SectionName = "Admin";

    public string? Email { get; set; }
    public string? Password { get; set; }
}
