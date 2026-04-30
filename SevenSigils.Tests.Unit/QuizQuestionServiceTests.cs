using FluentAssertions;
using SevenSigils.Application.Services;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Tests.Unit;

public sealed class QuizQuestionServiceTests
{
    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateQuestionAsync_ShouldReturnFourOptions_AndOneCorrect()
    {
        var sut = BuildSut(FiveBlazonRepo());

        var result = await sut.CreateQuestionAsync(Difficulty.Easy, []);

        result.Options.Should().HaveCount(4);
        result.Options.Should().Contain(result.CorrectOption);
        result.Options.Distinct(StringComparer.OrdinalIgnoreCase).Should().HaveCount(4);
    }

    [Fact]
    public async Task CreateQuestionAsync_ShouldWork_WithExactlyFourBlazon()
    {
        var sut = BuildSut(new FakeRepository(
            Create("a", "A"),
            Create("b", "B"),
            Create("c", "C"),
            Create("d", "D")));

        var result = await sut.CreateQuestionAsync(Difficulty.Easy, []);

        result.Options.Should().HaveCount(4);
        result.Options.Should().Contain(result.CorrectOption);
    }

    [Fact]
    public async Task CreateQuestionAsync_ShouldNotPickExcludedBlazon_AsTarget()
    {
        // Exclude all but "tyrell" — target must always be tyrell
        var sut = BuildSut(FiveBlazonRepo());
        string[] excluded = ["stark", "lannister", "targaryen", "baratheon"];

        var result = await sut.CreateQuestionAsync(Difficulty.Easy, excluded);

        result.Blazon.Id.Should().Be("tyrell");
        result.CorrectOption.Should().Be("Tyrell");
    }

    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Hard)]
    public async Task CreateQuestionAsync_ShouldWork_ForBothDifficulties(Difficulty difficulty)
    {
        var sut = BuildSut(FiveBlazonRepo());

        var result = await sut.CreateQuestionAsync(difficulty, []);

        result.Options.Should().HaveCount(4);
        result.Options.Should().Contain(result.CorrectOption);
    }

    // ── Error branches ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateQuestionAsync_ShouldThrow_WhenAllBlazonAreExcluded()
    {
        var sut = BuildSut(FiveBlazonRepo());
        string[] excluded = ["stark", "lannister", "targaryen", "baratheon", "tyrell"];

        var act = () => sut.CreateQuestionAsync(Difficulty.Easy, excluded);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No unseen blazons available*");
    }

    [Fact]
    public async Task CreateQuestionAsync_ShouldThrow_WhenPoolHasFewerThanFourBlazon()
    {
        var sut = BuildSut(new FakeRepository(
            Create("a", "A"),
            Create("b", "B"),
            Create("c", "C")));

        var act = () => sut.CreateQuestionAsync(Difficulty.Easy, []);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Not enough blazons*");
    }

    [Fact]
    public async Task CreateQuestionAsync_ShouldThrow_WhenNotEnoughDistinctDistractors()
    {
        // 4 blazons sharing the same FamilyLabel → can never produce 3 distinct distractors
        var sut = BuildSut(new FakeRepository(
            Create("a1", "Same"),
            Create("a2", "Same"),
            Create("a3", "Same"),
            Create("b",  "Other")));

        var act = () => sut.CreateQuestionAsync(Difficulty.Easy, []);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Unable to generate enough distractors*");
    }

    [Fact]
    public async Task CreateQuestionAsync_ShouldThrow_WhenCancellationRequested()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var sut = BuildSut(new CancellingFakeRepository());

        var act = () => sut.CreateQuestionAsync(Difficulty.Easy, [], cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static QuizQuestionService BuildSut(IBlazonRepository repo) =>
        new(repo, new FakeRandomProvider());

    private static FakeRepository FiveBlazonRepo() => new(
        Create("stark",     "Stark"),
        Create("lannister", "Lannister"),
        Create("targaryen", "Targaryen"),
        Create("baratheon", "Baratheon"),
        Create("tyrell",    "Tyrell"));

    private static Blazon Create(string id, string label) =>
        new(id, id, label, null, "https://example.test", "family", null,
            true, [], new Attribution("evrach", "https://example.test",
                "CC BY-SA 4.0", "https://creativecommons.org/licenses/by-sa/4.0/", null));

    private sealed class FakeRepository(params Blazon[] blazons) : IBlazonRepository
    {
        public Task<IReadOnlyList<Blazon>> GetByDifficultyAsync(
            Difficulty difficulty,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<Blazon>>(blazons);
        }
    }

    private sealed class CancellingFakeRepository : IBlazonRepository
    {
        public Task<IReadOnlyList<Blazon>> GetByDifficultyAsync(
            Difficulty difficulty,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<Blazon>>(Array.Empty<Blazon>());
        }
    }

    private sealed class FakeRandomProvider : IRandomProvider
    {
        private double _next = 0.1234;

        public double NextDouble()
        {
            _next += 0.1111;
            if (_next >= 1.0) _next = 0.1234;
            return _next;
        }
    }
}
