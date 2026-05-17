using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Admin;

public sealed record CreateBlazonCommand(
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
