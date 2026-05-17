using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;
using Testcontainers.MongoDb;

namespace SevenSigils.Tests.Integration;

// ── Tests ─────────────────────────────────────────────────────────────────────

public sealed class QuizApiTests : IClassFixture<QuizApiFactory>
{
    private readonly QuizApiFactory _factory;

    public QuizApiTests(QuizApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateQuestion_ShouldReturn200_WithEasyQuestion()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/quiz/question", new
        {
            difficulty = "easy",
            excludedIds = Array.Empty<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<QuestionBody>();
        body.Should().NotBeNull();
        body!.CorrectOption.Should().NotBeNullOrWhiteSpace();
        body.Options.Should().HaveCount(4);
        body.Options.Should().Contain(body.CorrectOption);
        body.Blazon.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateQuestion_ShouldReturn200_WithHardQuestion()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/quiz/question", new
        {
            difficulty = "hard",
            excludedIds = Array.Empty<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<QuestionBody>();
        body.Should().NotBeNull();
        body!.Options.Should().HaveCount(4);
        body.Blazon.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateQuestion_ShouldReturn200_WhenSomeIdsAreExcluded()
    {
        // Exclude 2 of the 6 seeded blazons — 4 unseen remain, enough for a question
        var excluded = QuizApiFactory.SeedSlugs.Take(2).ToArray();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/quiz/question", new
        {
            difficulty = "easy",
            excludedIds = excluded
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<QuestionBody>();
        body!.Options.Should().HaveCount(4);
    }

    [Fact]
    public async Task CreateQuestion_ShouldReturn400_WhenPoolIsExhausted()
    {
        // Exclude every seeded easy blazon — no unseen blazon left
        var allIds = QuizApiFactory.SeedSlugs.ToArray();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/quiz/question", new
        {
            difficulty = "easy",
            excludedIds = allIds
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────

    private sealed record QuestionBody(BlazonBody Blazon, List<string> Options, string CorrectOption);
    private sealed record BlazonBody(string Id, string FamilySlug, string FamilyLabel);
}

// ── Factory (Testcontainers + real MongoDB) ────────────────────────────────────

public sealed class QuizApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // 6 blazons seeded — 4 distractors needed by QuizQuestionService (OptionsCount = 4)
    public static readonly string[] SeedSlugs = ["stark", "lannister", "baratheon", "targaryen", "tully", "arryn"];

    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:7").Build();

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _mongoContainer.StartAsync();

        // Force the WebApplication host to build using the now-running container
        _ = CreateClient();

        // Seed test blazons into the real MongoDB instance via DI
        using var scope = Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBlazonRepository>();
        foreach (var slug in SeedSlugs)
            await repo.CreateAsync(BuildBlazon(slug), CancellationToken.None);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
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
                ["MongoDb:DatabaseName"] = "sevensigils_quiz_tests",
                ["MongoDb:SeedOnStartup"] = "false",
                ["Jwt:Key"] = "TEST_ONLY_LONG_ENOUGH_SECRET_KEY_1234567890",
                ["Jwt:Issuer"] = "SevenSigils.Tests",
                ["Jwt:Audience"] = "SevenSigils.Tests"
            });
        });
    }

    private static Blazon BuildBlazon(string slug) => new(
        Id: slug,
        FamilySlug: slug,
        FamilyLabel: char.ToUpperInvariant(slug[0]) + slug[1..],
        DisplayName: null,
        HousePageUrl: "https://example.test",
        Kind: null,
        VariantOf: null,
        IncludeInEasy: true,
        IncludeInHard: true,
        Hints: [],
        Attribution: new Attribution(
            Author: null,
            SourcePageUrl: "https://example.test",
            LicenseLabel: "CC BY-SA 4.0",
            LicenseUrl: "https://creativecommons.org/licenses/by-sa/4.0/",
            Notes: ""));
}
