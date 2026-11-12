using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;
using SevenSigils.Infrastructure.Seeding;
using Testcontainers.MongoDb;

namespace SevenSigils.Tests.Integration;

// Production : MongoDb:SeedMode=Replace. Le dépôt fait foi — un nouveau seed
// doit effacer tout ce qui a dérivé en base (blason ajouté, citation ajoutée,
// titre modifié) et remettre exactement les données de référence.
public sealed class SeedReplaceApiTests : IClassFixture<SeedReplaceApiFactory>
{
    private readonly SeedReplaceApiFactory _factory;

    public SeedReplaceApiTests(SeedReplaceApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Seed_ShouldReplaceDriftedData_WithReferenceData()
    {
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var blazons = services.GetRequiredService<IBlazonRepository>();
        var quotes = services.GetRequiredService<IQuoteRepository>();

        // Le seed du démarrage a déjà chargé le JSON de test.
        var (_, initialCount) = await blazons.GetAllAsync(1, 100);
        initialCount.Should().Be(SeedReplaceApiFactory.JsonSlugs.Length);

        // Dérive : données ajoutées ou modifiées hors du dépôt.
        await blazons.CreateAsync(BuildBlazon("intrus"));
        await quotes.CreateAsync(new CompetitiveQuote("extra", QuoteTiers.Grim, "Citation ajoutée à la main"));
        await quotes.UpsertTierTitleAsync(QuoteTiers.Legendary, "Titre modifié");

        await services.GetRequiredService<BlazonSeeder>().SeedAsync();
        await services.GetRequiredService<QuoteSeeder>().SeedAsync();

        var (items, total) = await blazons.GetAllAsync(1, 100);
        total.Should().Be(SeedReplaceApiFactory.JsonSlugs.Length);
        items.Select(b => b.FamilySlug).Should().BeEquivalentTo(SeedReplaceApiFactory.JsonSlugs);

        var allQuotes = await quotes.GetAllAsync();
        allQuotes.Should().HaveCount(12);
        allQuotes.Should().NotContain(q => q.Text == "Citation ajoutée à la main");

        var titles = await quotes.GetTierTitlesAsync();
        titles[QuoteTiers.Legendary].Should().Be("Mestre de la Citadelle");
    }

    private static Blazon BuildBlazon(string slug) => new(
        Id: slug,
        FamilySlug: slug,
        FamilyLabel: slug,
        DisplayName: null,
        HousePageUrl: "https://example.test",
        Kind: null,
        VariantOf: null,
        IncludeInEasy: false,
        IncludeInHard: true,
        Hints: [],
        Attribution: new Attribution(
            Author: null,
            SourcePageUrl: "https://example.test",
            LicenseLabel: "CC BY-SA 4.0",
            LicenseUrl: "https://creativecommons.org/licenses/by-sa/4.0/",
            Notes: ""));
}

public sealed class SeedReplaceApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static readonly string[] JsonSlugs = ["stark", "lannister"];

    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:7").Build();
    private readonly string _jsonPath = Path.Combine(Path.GetTempPath(), $"blazonDb-{Guid.NewGuid():N}.json");

    async Task IAsyncLifetime.InitializeAsync()
    {
        await File.WriteAllTextAsync(_jsonPath, """
            {
              "easyModeSlugs": ["stark"],
              "entries": {
                "stark": { "label": "Stark" },
                "lannister": { "label": "Lannister" }
              }
            }
            """);
        await _mongoContainer.StartAsync();

        // Démarre l'hôte : le seed Replace s'exécute au démarrage.
        _ = CreateClient();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
        File.Delete(_jsonPath);
        Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = _mongoContainer.GetConnectionString(),
                ["MongoDb:DatabaseName"] = "sevensigils_seed_replace_tests",
                ["MongoDb:SeedOnStartup"] = "true",
                ["MongoDb:SeedMode"] = "Replace",
                ["BlazonData:JsonPath"] = _jsonPath,
                ["Admin:Enabled"] = "false"
            });
        });
    }
}
