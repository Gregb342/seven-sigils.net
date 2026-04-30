using System.Text.Json;
using Microsoft.Extensions.Options;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;
using SevenSigils.Infrastructure.Options;

namespace SevenSigils.Infrastructure.Repositories;

public sealed class FileBlazonRepository : IBlazonRepository
{
    private readonly Lazy<IReadOnlyList<Blazon>> _cache;

    public FileBlazonRepository(IOptions<BlazonDataOptions> options)
    {
        var path = options.Value.JsonPath;
        _cache = new Lazy<IReadOnlyList<Blazon>>(() => LoadFromJson(path));
    }

    public Task<IReadOnlyList<Blazon>> GetByDifficultyAsync(Difficulty difficulty, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var all = _cache.Value;

        if (difficulty == Difficulty.Easy)
        {
            var easy = all.Where(x => x.Kind != "variant" && x.IncludeInHard).Take(31).ToList();
            return Task.FromResult<IReadOnlyList<Blazon>>(easy);
        }

        var hard = all.Where(x => x.IncludeInHard && x.Kind != "variant").ToList();
        return Task.FromResult<IReadOnlyList<Blazon>>(hard);
    }

    private static IReadOnlyList<Blazon> LoadFromJson(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Blazon database not found at '{path}'.");
        }

        var json = File.ReadAllText(path);
        var document = JsonSerializer.Deserialize<BlazonDbDocument>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (document?.Entries is null || document.Entries.Count == 0)
        {
            throw new InvalidOperationException("Blazon database is empty.");
        }

        return document.Entries
            .Select(kvp => MapEntry(kvp.Key, kvp.Value))
            .ToList();
    }

    private static Blazon MapEntry(string slug, BlazonDbEntry entry)
    {
        var label = string.IsNullOrWhiteSpace(entry.Label) ? Capitalize(slug) : entry.Label;
        var houseUrl = string.IsNullOrWhiteSpace(entry.HousePageUrl)
            ? $"https://lagardedenuit.com/wiki/index.php?title=Maison_{label.Replace(' ', '_')}"
            : entry.HousePageUrl;

        return new Blazon(
            Id: slug,
            FamilySlug: slug,
            FamilyLabel: label,
            DisplayName: entry.DisplayName,
            HousePageUrl: houseUrl,
            Kind: entry.Kind,
            VariantOf: entry.VariantOf,
            IncludeInHard: entry.IncludeInHard ?? !IsAltVariant(slug),
            Hints: (entry.Hints ?? new List<BlazonHint>()).Select(h => new HouseHint(h.Title ?? "Hint", h.Value ?? string.Empty)).ToList(),
            Attribution: new Attribution(
                Author: "Evrach",
                SourcePageUrl: houseUrl,
                LicenseLabel: "CC BY-SA 4.0",
                LicenseUrl: "https://creativecommons.org/licenses/by-sa/4.0/",
                Notes: "sauf mention contraire"));
    }

    private static bool IsAltVariant(string slug) =>
        slug.Contains("-alt", StringComparison.OrdinalIgnoreCase)
        || slug.StartsWith("alt", StringComparison.OrdinalIgnoreCase);

    private static string Capitalize(string value) =>
        string.IsNullOrWhiteSpace(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];

    private sealed class BlazonDbDocument
    {
        public List<string> EasyModeSlugs { get; set; } = new();
        public Dictionary<string, BlazonDbEntry> Entries { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    private sealed class BlazonDbEntry
    {
        public string? Label { get; set; }
        public string? DisplayName { get; set; }
        public string? Kind { get; set; }
        public string? VariantOf { get; set; }
        public bool? IncludeInHard { get; set; }
        public string? HousePageUrl { get; set; }
        public List<BlazonHint>? Hints { get; set; }
    }

    private sealed class BlazonHint
    {
        public string? Title { get; set; }
        public string? Value { get; set; }
    }
}
