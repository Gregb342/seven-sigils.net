using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Admin;

public sealed record UpdateBlazonCommand(
    string FamilyLabel,
    string? DisplayName,
    string HousePageUrl,
    string? Kind,
    string? VariantOf,
    bool IncludeInEasy,
    bool IncludeInHard,
    IReadOnlyList<HouseHint> Hints,
    Attribution Attribution);
