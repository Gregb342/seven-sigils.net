namespace SevenSigils.Api.Contracts.Admin;

public sealed record CreateBlazonRequest(
    string FamilySlug,
    string FamilyLabel,
    string? DisplayName,
    string HousePageUrl,
    string? Kind,
    string? VariantOf,
    bool IncludeInEasy,
    bool IncludeInHard,
    IReadOnlyList<HouseHintRequest> Hints,
    AttributionRequest Attribution);

public sealed record HouseHintRequest(string Title, string Value);

public sealed record AttributionRequest(
    string? Author,
    string SourcePageUrl,
    string LicenseLabel,
    string LicenseUrl,
    string? Notes);
