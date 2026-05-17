using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;
using SevenSigils.Infrastructure.Documents;
using SevenSigils.Infrastructure.Options;

namespace SevenSigils.Infrastructure.Repositories;

public sealed class MongoDbBlazonRepository : IBlazonRepository
{
    private readonly IMongoCollection<BlazonDocument> _collection;

    public MongoDbBlazonRepository(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var database = mongoClient.GetDatabase(options.Value.DatabaseName);
        _collection = database.GetCollection<BlazonDocument>(options.Value.BlazonCollection);
    }

    public async Task<IReadOnlyList<Blazon>> GetByDifficultyAsync(
        Difficulty difficulty,
        CancellationToken cancellationToken = default)
    {
        var notVariant = Builders<BlazonDocument>.Filter.Ne(x => x.Kind, "variant");

        var difficultyFilter = difficulty == Difficulty.Easy
            ? Builders<BlazonDocument>.Filter.Eq(x => x.IsEasy, true)
            : Builders<BlazonDocument>.Filter.Eq(x => x.IncludeInHard, true);

        var filter = Builders<BlazonDocument>.Filter.And(notVariant, difficultyFilter);

        var documents = await _collection.Find(filter).ToListAsync(cancellationToken);

        return documents.Select(ToDomain).ToList();
    }

    public async Task<(IReadOnlyList<Blazon> Items, long TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<BlazonDocument>.Filter.Empty;

        var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        var documents = await _collection
            .Find(filter)
            .SortBy(x => x.FamilySlug)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return (documents.Select(ToDomain).ToList(), totalCount);
    }

    public async Task<Blazon?> GetBySlugAsync(string familySlug, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BlazonDocument>.Filter.Eq(x => x.FamilySlug, familySlug);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToDomain(document);
    }

    public async Task<Blazon> CreateAsync(Blazon blazon, CancellationToken cancellationToken = default)
    {
        var document = ToDocument(blazon);
        await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        return blazon;
    }

    public async Task<Blazon?> UpdateAsync(Blazon blazon, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BlazonDocument>.Filter.Eq(x => x.FamilySlug, blazon.FamilySlug);
        var document = ToDocument(blazon);
        var result = await _collection.ReplaceOneAsync(filter, document, cancellationToken: cancellationToken);
        return result.MatchedCount > 0 ? blazon : null;
    }

    public async Task<bool> DeleteAsync(string familySlug, CancellationToken cancellationToken = default)
    {
        var filter = Builders<BlazonDocument>.Filter.Eq(x => x.FamilySlug, familySlug);
        var result = await _collection.DeleteOneAsync(filter, cancellationToken);
        return result.DeletedCount > 0;
    }

    private static Blazon ToDomain(BlazonDocument doc) => new(
        Id: doc.Id,
        FamilySlug: doc.FamilySlug,
        FamilyLabel: doc.FamilyLabel,
        DisplayName: doc.DisplayName,
        HousePageUrl: doc.HousePageUrl,
        Kind: doc.Kind,
        VariantOf: doc.VariantOf,
        IncludeInEasy: doc.IsEasy,
        IncludeInHard: doc.IncludeInHard,
        Hints: doc.Hints
            .Select(h => new HouseHint(h.Title, h.Value))
            .ToList(),
        Attribution: new Attribution(
            Author: doc.Attribution.Author,
            SourcePageUrl: doc.Attribution.SourcePageUrl,
            LicenseLabel: doc.Attribution.LicenseLabel,
            LicenseUrl: doc.Attribution.LicenseUrl,
            Notes: doc.Attribution.Notes ?? string.Empty));

    private static BlazonDocument ToDocument(Blazon blazon) => new()
    {
        Id = blazon.Id,
        FamilySlug = blazon.FamilySlug,
        FamilyLabel = blazon.FamilyLabel,
        DisplayName = blazon.DisplayName,
        HousePageUrl = blazon.HousePageUrl,
        Kind = blazon.Kind,
        VariantOf = blazon.VariantOf,
        IsEasy = blazon.IncludeInEasy,
        IncludeInHard = blazon.IncludeInHard,
        Hints = blazon.Hints
            .Select(h => new HouseHintDocument { Title = h.Title, Value = h.Value })
            .ToList(),
        Attribution = new AttributionDocument
        {
            Author = blazon.Attribution.Author,
            SourcePageUrl = blazon.Attribution.SourcePageUrl,
            LicenseLabel = blazon.Attribution.LicenseLabel,
            LicenseUrl = blazon.Attribution.LicenseUrl,
            Notes = blazon.Attribution.Notes
        }
    };
}
