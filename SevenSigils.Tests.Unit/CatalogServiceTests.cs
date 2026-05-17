using FluentAssertions;
using SevenSigils.Application.Catalog;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Tests.Unit;

public sealed class CatalogServiceTests
{
    // ── GetPageAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPageAsync_ShouldReturnPagedResult_WithCorrectItems()
    {
        var repo = new FakeBlazonRepository(Create("stark"), Create("lannister"), Create("targaryen"));
        var sut = new CatalogService(repo);

        var result = await sut.GetPageAsync(1, 10);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task GetPageAsync_ShouldReturnCorrectTotalPages_WhenMultiplePages()
    {
        var blazons = Enumerable.Range(1, 25).Select(i => Create($"house-{i}")).ToArray();
        var repo = new FakeBlazonRepository(blazons);
        var sut = new CatalogService(repo);

        var result = await sut.GetPageAsync(2, 10);

        result.TotalCount.Should().Be(25);
        result.TotalPages.Should().Be(3);
        result.Page.Should().Be(2);
    }

    [Fact]
    public async Task GetPageAsync_ShouldMapDomainToBlazonDto()
    {
        var blazon = Create("stark", "Stark");
        var repo = new FakeBlazonRepository(blazon);
        var sut = new CatalogService(repo);

        var result = await sut.GetPageAsync(1, 10);

        var dto = result.Items.Single();
        dto.FamilySlug.Should().Be("stark");
        dto.FamilyLabel.Should().Be("Stark");
        dto.IncludeInEasy.Should().BeTrue();
        dto.IncludeInHard.Should().BeTrue();
    }

    // ── GetBySlugAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnDto_WhenBlazonExists()
    {
        var repo = new FakeBlazonRepository(Create("stark", "Stark"));
        var sut = new CatalogService(repo);

        var result = await sut.GetBySlugAsync("stark");

        result.Should().NotBeNull();
        result!.FamilySlug.Should().Be("stark");
        result.FamilyLabel.Should().Be("Stark");
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenBlazonDoesNotExist()
    {
        var repo = new FakeBlazonRepository();
        var sut = new CatalogService(repo);

        var result = await sut.GetBySlugAsync("unknown");

        result.Should().BeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Blazon Create(string slug, string? label = null) =>
        new(slug, slug, label ?? slug, null, "https://example.test", null, null,
            true, true, [], new Attribution(null, "https://example.test",
                "CC BY-SA 4.0", "https://creativecommons.org/licenses/by-sa/4.0/", null));
}

internal sealed class FakeBlazonRepository(params Blazon[] seed) : IBlazonRepository
{
    private readonly List<Blazon> _store = [..seed];

    public Task<IReadOnlyList<Blazon>> GetByDifficultyAsync(
        Difficulty difficulty, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Blazon>>(_store);

    public Task<(IReadOnlyList<Blazon> Items, long TotalCount)> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = _store.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<Blazon>, long)>((items, _store.Count));
    }

    public Task<Blazon?> GetBySlugAsync(string familySlug, CancellationToken cancellationToken = default) =>
        Task.FromResult(_store.FirstOrDefault(b => b.FamilySlug == familySlug));

    public Task<Blazon> CreateAsync(Blazon blazon, CancellationToken cancellationToken = default)
    {
        _store.Add(blazon);
        return Task.FromResult(blazon);
    }

    public Task<Blazon?> UpdateAsync(Blazon blazon, CancellationToken cancellationToken = default)
    {
        var idx = _store.FindIndex(b => b.FamilySlug == blazon.FamilySlug);
        if (idx < 0) return Task.FromResult<Blazon?>(null);
        _store[idx] = blazon;
        return Task.FromResult<Blazon?>(blazon);
    }

    public Task<bool> DeleteAsync(string familySlug, CancellationToken cancellationToken = default)
    {
        var removed = _store.RemoveAll(b => b.FamilySlug == familySlug);
        return Task.FromResult(removed > 0);
    }
}
