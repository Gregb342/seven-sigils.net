namespace SevenSigils.Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = "CHANGE_ME_WITH_A_LONGER_SECRET_KEY_32+";
    public string Issuer { get; set; } = "SevenSigils";
    public string Audience { get; set; } = "SevenSigils";
    public int ExpiryMinutes { get; set; } = 60;
}