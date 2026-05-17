using SevenSigils.Domain.Models;

namespace SevenSigils.Domain.Abstractions;

public interface IBlazonRepository
{
    Task<IReadOnlyList<Blazon>> GetByDifficultyAsync(Difficulty difficulty, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Blazon> Items, long TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Blazon?> GetBySlugAsync(string familySlug, CancellationToken cancellationToken = default);

    Task<Blazon> CreateAsync(Blazon blazon, CancellationToken cancellationToken = default);

    Task<Blazon?> UpdateAsync(Blazon blazon, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string familySlug, CancellationToken cancellationToken = default);
}
