using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Quotes;

public sealed class QuoteService : IQuoteService
{
    private readonly IQuoteRepository _repository;

    public QuoteService(IQuoteRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<CompetitiveQuote>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(cancellationToken);

    public Task<CompetitiveQuote> CreateAsync(string tier, string text, CancellationToken cancellationToken = default)
    {
        var quote = new CompetitiveQuote(
            Id: Guid.NewGuid().ToString("N"),
            Tier: tier,
            Text: text.Trim());

        return _repository.CreateAsync(quote, cancellationToken);
    }

    public async Task<CompetitiveQuote> UpdateAsync(string id, string tier, string text, CancellationToken cancellationToken = default)
    {
        var updated = await _repository.UpdateAsync(
            new CompetitiveQuote(id, tier, text.Trim()),
            cancellationToken);

        return updated ?? throw new QuoteNotFoundException(id);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
            throw new QuoteNotFoundException(id);
    }

    public Task<IReadOnlyDictionary<string, string>> GetTierTitlesAsync(CancellationToken cancellationToken = default) =>
        _repository.GetTierTitlesAsync(cancellationToken);

    public Task UpdateTierTitleAsync(string tier, string title, CancellationToken cancellationToken = default) =>
        _repository.UpsertTierTitleAsync(tier, title.Trim(), cancellationToken);
}
