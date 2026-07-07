using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Admin;

/// <summary>
/// Instantané de la collection au format blazonDb.json : Mongo étant la source
/// de vérité, cet export sert de sauvegarde/archive re-versionnable dans Git,
/// et reste réimportable par BlazonSeeder (qui ignore le champ attribution,
/// ajouté ici pour la fidélité de la sauvegarde).
/// </summary>
public sealed record BlazonExport(
    IReadOnlyList<string> EasyModeSlugs,
    IReadOnlyDictionary<string, BlazonExportEntry> Entries);

public sealed record BlazonExportEntry(
    string Label,
    string? DisplayName,
    string? Kind,
    string? VariantOf,
    bool IncludeInHard,
    string HousePageUrl,
    IReadOnlyList<HouseHint> Hints,
    Attribution Attribution);
