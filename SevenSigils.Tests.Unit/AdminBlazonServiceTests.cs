using FluentAssertions;
using SevenSigils.Application.Admin;
using SevenSigils.Domain.Models;

namespace SevenSigils.Tests.Unit;

public sealed class AdminBlazonServiceTests
{
    private static readonly Attribution SampleAttribution = new(
        Author: "Evrach",
        SourcePageUrl: "https://example.test",
        LicenseLabel: "CC BY-SA 4.0",
        LicenseUrl: "https://creativecommons.org/licenses/by-sa/4.0/",
        Notes: null);

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldReturnDto_WhenSlugIsNew()
    {
        var repo = new FakeBlazonRepository();
        var sut = new AdminBlazonService(repo);

        var result = await sut.CreateAsync(SampleCreateCommand("stark"));

        result.FamilySlug.Should().Be("stark");
        result.FamilyLabel.Should().Be("Stark");
        result.Id.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowSlugAlreadyExistsException_WhenSlugIsTaken()
    {
        var repo = new FakeBlazonRepository(CreateBlazon("stark"));
        var sut = new AdminBlazonService(repo);

        var act = () => sut.CreateAsync(SampleCreateCommand("stark"));

        await act.Should().ThrowAsync<SlugAlreadyExistsException>()
            .WithMessage("*stark*");
    }

    // ── ExportAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ExportAsync_ShouldReturnEntriesKeyedBySlug_WithEasyModeSlugs()
    {
        var easy = CreateBlazon("stark", "Stark") with { IncludeInEasy = true };
        var hardOnly = CreateBlazon("aemonrivers", "Aemonrivers") with { IncludeInEasy = false };
        var repo = new FakeBlazonRepository(easy, hardOnly);
        var sut = new AdminBlazonService(repo);

        var export = await sut.ExportAsync();

        export.EasyModeSlugs.Should().ContainSingle().Which.Should().Be("stark");
        export.Entries.Keys.Should().BeEquivalentTo("aemonrivers", "stark");
        export.Entries["stark"].Label.Should().Be("Stark");
        export.Entries["stark"].Attribution.Should().Be(easy.Attribution);
    }

    [Fact]
    public async Task ExportAsync_ShouldPageThroughTheWholeCollection()
    {
        // 250 éléments > pageSize interne (200) : vérifie que la boucle de pagination agrège tout.
        var many = Enumerable.Range(1, 250).Select(i => CreateBlazon($"house-{i:D3}")).ToArray();
        var repo = new FakeBlazonRepository(many);
        var sut = new AdminBlazonService(repo);

        var export = await sut.ExportAsync();

        export.Entries.Should().HaveCount(250);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldReturnUpdatedDto_WhenBlazonExists()
    {
        var repo = new FakeBlazonRepository(CreateBlazon("stark", "Stark"));
        var sut = new AdminBlazonService(repo);

        var result = await sut.UpdateAsync("stark", SampleUpdateCommand("Stark Updated"));

        result.FamilyLabel.Should().Be("Stark Updated");
        result.FamilySlug.Should().Be("stark");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowBlazonNotFoundException_WhenBlazonDoesNotExist()
    {
        var repo = new FakeBlazonRepository();
        var sut = new AdminBlazonService(repo);

        var act = () => sut.UpdateAsync("unknown", SampleUpdateCommand("X"));

        await act.Should().ThrowAsync<BlazonNotFoundException>()
            .WithMessage("*unknown*");
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ShouldSucceed_WhenBlazonExists()
    {
        var repo = new FakeBlazonRepository(CreateBlazon("stark"));
        var sut = new AdminBlazonService(repo);

        var act = () => sut.DeleteAsync("stark");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowBlazonNotFoundException_WhenBlazonDoesNotExist()
    {
        var repo = new FakeBlazonRepository();
        var sut = new AdminBlazonService(repo);

        var act = () => sut.DeleteAsync("unknown");

        await act.Should().ThrowAsync<BlazonNotFoundException>()
            .WithMessage("*unknown*");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Blazon CreateBlazon(string slug, string? label = null) =>
        new(slug, slug, label ?? slug, null, "https://example.test", null, null,
            true, true, [], SampleAttribution);

    private static CreateBlazonCommand SampleCreateCommand(string slug) => new(
        FamilySlug: slug,
        FamilyLabel: char.ToUpperInvariant(slug[0]) + slug[1..],
        DisplayName: null,
        HousePageUrl: "https://example.test",
        Kind: null,
        VariantOf: null,
        IncludeInEasy: true,
        IncludeInHard: true,
        Hints: [],
        Attribution: SampleAttribution);

    private static UpdateBlazonCommand SampleUpdateCommand(string label) => new(
        FamilyLabel: label,
        DisplayName: null,
        HousePageUrl: "https://example.test",
        Kind: null,
        VariantOf: null,
        IncludeInEasy: true,
        IncludeInHard: true,
        Hints: [],
        Attribution: SampleAttribution);
}
