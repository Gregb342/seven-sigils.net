using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Services;

public interface IQuizQuestionService
{
    Task<Question> CreateQuestionAsync(
        Difficulty difficulty,
        IReadOnlyCollection<string> excludedIds,
        CancellationToken cancellationToken = default);
}
