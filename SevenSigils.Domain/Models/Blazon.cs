namespace SevenSigils.Domain.Models;

public sealed record Blazon(
    string Id,
    string FamilySlug,
    string FamilyLabel,
    string? DisplayName,
    string HousePageUrl,
    string? Kind,
    string? VariantOf,
    bool IncludeInEasy,
    bool IncludeInHard,
    IReadOnlyList<HouseHint> Hints,
    Attribution Attribution);
