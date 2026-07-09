using SevenSigils.Domain.Models;

namespace SevenSigils.Domain.Abstractions;

public interface IQuoteRepository
{
    Task<IReadOnlyList<CompetitiveQuote>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CompetitiveQuote?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<CompetitiveQuote> CreateAsync(CompetitiveQuote quote, CancellationToken cancellationToken = default);

    Task<CompetitiveQuote?> UpdateAsync(CompetitiveQuote quote, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
