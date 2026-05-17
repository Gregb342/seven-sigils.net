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

    private static Blazon ToDomain(BlazonDocument doc) => new(
        Id: doc.Id,
        FamilySlug: doc.FamilySlug,
        FamilyLabel: doc.FamilyLabel,
        DisplayName: doc.DisplayName,
        HousePageUrl: doc.HousePageUrl,
        Kind: doc.Kind,
        VariantOf: doc.VariantOf,
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
}
