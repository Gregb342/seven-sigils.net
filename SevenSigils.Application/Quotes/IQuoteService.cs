using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Quotes;

public interface IQuoteService
{
    Task<IReadOnlyList<CompetitiveQuote>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CompetitiveQuote> CreateAsync(string tier, string text, CancellationToken cancellationToken = default);

    Task<CompetitiveQuote> UpdateAsync(string id, string tier, string text, CancellationToken cancellationToken = default);

    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, string>> GetTierTitlesAsync(CancellationToken cancellationToken = default);

    Task UpdateTierTitleAsync(string tier, string title, CancellationToken cancellationToken = default);
}
