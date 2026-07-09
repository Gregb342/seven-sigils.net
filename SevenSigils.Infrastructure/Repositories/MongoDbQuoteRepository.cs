using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;
using SevenSigils.Infrastructure.Documents;
using SevenSigils.Infrastructure.Options;

namespace SevenSigils.Infrastructure.Repositories;

public sealed class MongoDbQuoteRepository : IQuoteRepository
{
    private readonly IMongoCollection<QuoteDocument> _collection;
    private readonly IMongoCollection<TierTitleDocument> _titleCollection;

    public MongoDbQuoteRepository(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var database = mongoClient.GetDatabase(options.Value.DatabaseName);
        _collection = database.GetCollection<QuoteDocument>(options.Value.QuoteCollection);
        _titleCollection = database.GetCollection<TierTitleDocument>(options.Value.TierTitleCollection);
    }

    public async Task<IReadOnlyList<CompetitiveQuote>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _collection
            .Find(FilterDefinition<QuoteDocument>.Empty)
            .SortBy(x => x.Tier)
            .ToListAsync(cancellationToken);

        return documents.Select(ToDomain).ToList();
    }

    public async Task<CompetitiveQuote?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var document = await _collection.Find(x => x.Id == id).SingleOrDefaultAsync(cancellationToken);
        return document is null ? null : ToDomain(document);
    }

    public async Task<CompetitiveQuote> CreateAsync(CompetitiveQuote quote, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(ToDocument(quote), cancellationToken: cancellationToken);
        return quote;
    }

    public async Task<CompetitiveQuote?> UpdateAsync(CompetitiveQuote quote, CancellationToken cancellationToken = default)
    {
        var result = await _collection.ReplaceOneAsync(
            x => x.Id == quote.Id,
            ToDocument(quote),
            cancellationToken: cancellationToken);

        return result.MatchedCount == 0 ? null : quote;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken);
        return result.DeletedCount > 0;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetTierTitlesAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _titleCollection
            .Find(FilterDefinition<TierTitleDocument>.Empty)
            .ToListAsync(cancellationToken);

        return documents.ToDictionary(d => d.Tier, d => d.Title);
    }

    public Task UpsertTierTitleAsync(string tier, string title, CancellationToken cancellationToken = default) =>
        _titleCollection.ReplaceOneAsync(
            x => x.Tier == tier,
            new TierTitleDocument { Tier = tier, Title = title },
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);

    private static CompetitiveQuote ToDomain(QuoteDocument doc) => new(doc.Id, doc.Tier, doc.Text);

    private static QuoteDocument ToDocument(CompetitiveQuote quote) => new()
    {
        Id = quote.Id,
        Tier = quote.Tier,
        Text = quote.Text
    };
}
