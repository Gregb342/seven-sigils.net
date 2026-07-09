using FluentAssertions;
using SevenSigils.Application.Quotes;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Tests.Unit;

public sealed class QuoteServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldGenerateIdAndTrimText()
    {
        var repo = new FakeQuoteRepository();
        var sut = new QuoteService(repo);

        var created = await sut.CreateAsync(QuoteTiers.Legendary, "  Le savoir est une arme.  ");

        created.Id.Should().NotBeNullOrWhiteSpace();
        created.Tier.Should().Be(QuoteTiers.Legendary);
        created.Text.Should().Be("Le savoir est une arme.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReplaceQuote_WhenItExists()
    {
        var repo = new FakeQuoteRepository(new CompetitiveQuote("q1", QuoteTiers.Grim, "Ancien texte"));
        var sut = new QuoteService(repo);

        var updated = await sut.UpdateAsync("q1", QuoteTiers.Strong, "Nouveau texte");

        updated.Tier.Should().Be(QuoteTiers.Strong);
        updated.Text.Should().Be("Nouveau texte");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenQuoteDoesNotExist()
    {
        var sut = new QuoteService(new FakeQuoteRepository());

        var act = () => sut.UpdateAsync("ghost", QuoteTiers.Grim, "X");

        await act.Should().ThrowAsync<QuoteNotFoundException>().WithMessage("*ghost*");
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenQuoteDoesNotExist()
    {
        var sut = new QuoteService(new FakeQuoteRepository());

        var act = () => sut.DeleteAsync("ghost");

        await act.Should().ThrowAsync<QuoteNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveQuote_WhenItExists()
    {
        var repo = new FakeQuoteRepository(new CompetitiveQuote("q1", QuoteTiers.Grim, "Texte"));
        var sut = new QuoteService(repo);

        await sut.DeleteAsync("q1");

        (await sut.GetAllAsync()).Should().BeEmpty();
    }

    private sealed class FakeQuoteRepository(params CompetitiveQuote[] seed) : IQuoteRepository
    {
        private readonly List<CompetitiveQuote> _store = [.. seed];

        public Task<IReadOnlyList<CompetitiveQuote>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CompetitiveQuote>>([.. _store]);

        public Task<CompetitiveQuote?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_store.FirstOrDefault(q => q.Id == id));

        public Task<CompetitiveQuote> CreateAsync(CompetitiveQuote quote, CancellationToken cancellationToken = default)
        {
            _store.Add(quote);
            return Task.FromResult(quote);
        }

        public Task<CompetitiveQuote?> UpdateAsync(CompetitiveQuote quote, CancellationToken cancellationToken = default)
        {
            var index = _store.FindIndex(q => q.Id == quote.Id);
            if (index < 0) return Task.FromResult<CompetitiveQuote?>(null);
            _store[index] = quote;
            return Task.FromResult<CompetitiveQuote?>(quote);
        }

        public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_store.RemoveAll(q => q.Id == id) > 0);
    }
}
