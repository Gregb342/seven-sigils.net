namespace SevenSigils.Api.Contracts.Catalog;

public sealed record BlazonResponse(
    string Id,
    string FamilySlug,
    string FamilyLabel,
    string? DisplayName,
    string HousePageUrl,
    string? Kind,
    string? VariantOf,
    bool IncludeInEasy,
    bool IncludeInHard,
    IReadOnlyList<HouseHintResponse> Hints,
    AttributionResponse Attribution);

public sealed record HouseHintResponse(string Title, string Value);

public sealed record AttributionResponse(
    string? Author,
    string SourcePageUrl,
    string LicenseLabel,
    string LicenseUrl,
    string? Notes);
