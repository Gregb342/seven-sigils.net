using SevenSigils.Domain.Models;

namespace SevenSigils.Domain.Abstractions;

public interface IBlazonRepository
{
    Task<IReadOnlyList<Blazon>> GetByDifficultyAsync(Difficulty difficulty, CancellationToken cancellationToken = default);
}
