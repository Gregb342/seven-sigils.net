using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Services;

public sealed class QuizQuestionService : IQuizQuestionService
{
    private const int OptionsCount = 4;

    private readonly IBlazonRepository _blazonRepository;
    private readonly IRandomProvider _randomProvider;

    public QuizQuestionService(IBlazonRepository blazonRepository, IRandomProvider randomProvider)
    {
        _blazonRepository = blazonRepository;
        _randomProvider = randomProvider;
    }

    public async Task<Question> CreateQuestionAsync(
        Difficulty difficulty,
        IReadOnlyCollection<string> excludedIds,
        CancellationToken cancellationToken = default)
    {
        var pool = await _blazonRepository.GetByDifficultyAsync(difficulty, cancellationToken);
        var unseen = pool.Where(x => !excludedIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).ToList();

        if (unseen.Count == 0)
        {
            throw new InvalidOperationException("No unseen blazons available.");
        }

        if (pool.Count < OptionsCount)
        {
            throw new InvalidOperationException("Not enough blazons to create a question.");
        }

        var target = PickRandom(unseen);
        var distractors = pool
            .Where(x => !string.Equals(x.FamilyLabel, target.FamilyLabel, StringComparison.OrdinalIgnoreCase))
            .OrderBy(_ => _randomProvider.NextDouble())
            .Take(OptionsCount - 1)
            .Select(x => x.FamilyLabel)
            .ToList();

        if (distractors.Count < OptionsCount - 1)
        {
            throw new InvalidOperationException("Unable to generate enough distractors.");
        }

        var options = distractors.Append(target.FamilyLabel)
            .OrderBy(_ => _randomProvider.NextDouble())
            .ToList();

        return new Question(target, options, target.FamilyLabel);
    }

    private Blazon PickRandom(IReadOnlyList<Blazon> items)
    {
        var index = (int)Math.Floor(_randomProvider.NextDouble() * items.Count);
        index = Math.Clamp(index, 0, items.Count - 1);
        return items[index];
    }
}
