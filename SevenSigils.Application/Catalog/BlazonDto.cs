using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Catalog;

public sealed record BlazonDto(
    string Id,
    string FamilySlug,
    string FamilyLabel,
    string? DisplayName,
    string HousePageUrl,
    string? Kind,
    string? VariantOf,
    bool IncludeInEasy,
    bool IncludeInHard,
    IReadOnlyList<HouseHintDto> Hints,
    AttributionDto Attribution)
{
    internal static BlazonDto From(Blazon blazon) => new(
        Id: blazon.Id,
        FamilySlug: blazon.FamilySlug,
        FamilyLabel: blazon.FamilyLabel,
        DisplayName: blazon.DisplayName,
        HousePageUrl: blazon.HousePageUrl,
        Kind: blazon.Kind,
        VariantOf: blazon.VariantOf,
        IncludeInEasy: blazon.IncludeInEasy,
        IncludeInHard: blazon.IncludeInHard,
        Hints: blazon.Hints.Select(h => new HouseHintDto(h.Title, h.Value)).ToList(),
        Attribution: new AttributionDto(
            blazon.Attribution.Author,
            blazon.Attribution.SourcePageUrl,
            blazon.Attribution.LicenseLabel,
            blazon.Attribution.LicenseUrl,
            blazon.Attribution.Notes));
}

public sealed record HouseHintDto(string Title, string Value);

public sealed record AttributionDto(
    string? Author,
    string SourcePageUrl,
    string LicenseLabel,
    string LicenseUrl,
    string? Notes);
