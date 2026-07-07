namespace SevenSigils.Domain.Models;

/// <summary>Citation affichée en fin de partie compétitive, rattachée à un palier de score.</summary>
public sealed record CompetitiveQuote(
    string Id,
    string Tier,
    string Text);

public static class QuoteTiers
{
    public const string Legendary = "legendary";
    public const string Strong = "strong";
    public const string Average = "average";
    public const string Grim = "grim";

    public static readonly IReadOnlyList<string> All = [Legendary, Strong, Average, Grim];

    public static bool IsValid(string tier) => All.Contains(tier);
}
