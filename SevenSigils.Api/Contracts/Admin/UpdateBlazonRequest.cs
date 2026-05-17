namespace SevenSigils.Api.Contracts.Admin;

public sealed record UpdateBlazonRequest(
    string FamilyLabel,
    string? DisplayName,
    string HousePageUrl,
    string? Kind,
    string? VariantOf,
    bool IncludeInEasy,
    bool IncludeInHard,
    IReadOnlyList<HouseHintRequest> Hints,
    AttributionRequest Attribution);
