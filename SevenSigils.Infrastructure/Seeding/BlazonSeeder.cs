using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SevenSigils.Infrastructure.Documents;
using SevenSigils.Infrastructure.Options;

namespace SevenSigils.Infrastructure.Seeding;

public sealed class BlazonSeeder
{
    private readonly IMongoCollection<BlazonDocument> _collection;
    private readonly string _jsonPath;
    private readonly ILogger<BlazonSeeder> _logger;

    public BlazonSeeder(
        IMongoClient mongoClient,
        IOptions<MongoDbOptions> mongoOptions,
        IOptions<BlazonDataOptions> dataOptions,
        ILogger<BlazonSeeder> logger)
    {
        var database = mongoClient.GetDatabase(mongoOptions.Value.DatabaseName);
        _collection = database.GetCollection<BlazonDocument>(mongoOptions.Value.BlazonCollection);
        _jsonPath = dataOptions.Value.JsonPath;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _collection.CountDocumentsAsync(
            FilterDefinition<BlazonDocument>.Empty,
            cancellationToken: cancellationToken);

        if (existing > 0)
        {
            _logger.LogInformation("Blazon collection already contains {Count} documents — skipping seed.", existing);
            return;
        }

        if (!File.Exists(_jsonPath))
        {
            _logger.LogError("Seed source not found at '{Path}'.", _jsonPath);
            throw new FileNotFoundException($"Blazon database not found at '{_jsonPath}'.");
        }

        var json = await File.ReadAllTextAsync(_jsonPath, cancellationToken);
        var document = JsonSerializer.Deserialize<BlazonDbDocument>(json, JsonOptions);

        if (document?.Entries is null || document.Entries.Count == 0)
            throw new InvalidOperationException("Blazon database is empty or could not be parsed.");

        var easySet = new HashSet<string>(document.EasyModeSlugs, StringComparer.OrdinalIgnoreCase);

        var docs = document.Entries
            .Select(kvp => MapToDocument(kvp.Key, kvp.Value, easySet))
            .ToList();

        await _collection.InsertManyAsync(docs, cancellationToken: cancellationToken);

        _logger.LogInformation("Seeded {Count} blazon documents into MongoDB.", docs.Count);
    }

    private static BlazonDocument MapToDocument(string slug, BlazonDbEntry entry, HashSet<string> easySet)
    {
        var label = string.IsNullOrWhiteSpace(entry.Label) ? Capitalize(slug) : entry.Label;
        var houseUrl = string.IsNullOrWhiteSpace(entry.HousePageUrl)
            ? $"https://lagardedenuit.com/wiki/index.php?title=Maison_{label.Replace(' ', '_')}"
            : entry.HousePageUrl;
        var includeInHard = entry.IncludeInHard ?? !IsAltVariant(slug);

        return new BlazonDocument
        {
            Id = slug,
            FamilySlug = slug,
            FamilyLabel = label,
            DisplayName = entry.DisplayName,
            HousePageUrl = houseUrl,
            Kind = entry.Kind,
            VariantOf = entry.VariantOf,
            IncludeInHard = includeInHard,
            IsEasy = easySet.Contains(slug),
            Hints = (entry.Hints ?? [])
                .Select(h => new HouseHintDocument
                {
                    Title = h.Title ?? "Hint",
                    Value = h.Value ?? string.Empty
                })
                .ToList(),
            Attribution = new AttributionDocument
            {
                Author = "Evrach",
                SourcePageUrl = houseUrl,
                LicenseLabel = "CC BY-SA 4.0",
                LicenseUrl = "https://creativecommons.org/licenses/by-sa/4.0/",
                Notes = "sauf mention contraire"
            }
        };
    }

    private static bool IsAltVariant(string slug) =>
        slug.Contains("-alt", StringComparison.OrdinalIgnoreCase)
        || slug.StartsWith("alt", StringComparison.OrdinalIgnoreCase);

    private static string Capitalize(string value) =>
        string.IsNullOrWhiteSpace(value) ? value : char.ToUpperInvariant(value[0]) + value[1..];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ── JSON mapping types ────────────────────────────────────────────────────

    private sealed class BlazonDbDocument
    {
        public List<string> EasyModeSlugs { get; set; } = [];
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
