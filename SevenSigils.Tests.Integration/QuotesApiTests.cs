using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Tests.Integration;

public sealed class QuotesApiTests : IClassFixture<QuotesApiFactory>
{
    private readonly QuotesApiFactory _factory;

    public QuotesApiTests(QuotesApiFactory factory)
    {
        _factory = factory;
    }

    // Lecture publique : le frontend consomme les citations sans compte.
    [Fact]
    public async Task GetAll_ShouldReturnQuotes_WhenNotAuthenticated()
    {
        _factory.SeedQuote(new CompetitiveQuote("q-read", QuoteTiers.Grim, "La nuit est sombre."));
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/quotes");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<QuoteBody>>();
        body.Should().Contain(q => q.Id == "q-read" && q.Tier == "grim");
    }

    [Fact]
    public async Task Create_ShouldReturn401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/quotes", new
        {
            tier = "legendary",
            text = "Tentative anonyme"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenUserRoleIsInsufficient()
    {
        var client = _factory.CreateUserClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/quotes", new
        {
            tier = "legendary",
            text = "Tentative sans rôle admin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WhenAdmin()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/quotes", new
        {
            tier = "strong",
            text = "Le Nord se souvient."
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<QuoteBody>();
        body!.Tier.Should().Be("strong");
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenTierIsUnknown()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/quotes", new
        {
            tier = "nightmare",
            text = "Palier inexistant"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenQuoteDoesNotExist()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.PutAsJsonAsync("/api/v1/admin/quotes/ghost", new
        {
            tier = "grim",
            text = "Fantôme"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenAdminDeletesExistingQuote()
    {
        _factory.SeedQuote(new CompetitiveQuote("q-delete", QuoteTiers.Average, "À supprimer"));
        var client = _factory.CreateAdminClient();

        var response = await client.DeleteAsync("/api/v1/admin/quotes/q-delete");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed record QuoteBody(string Id, string Tier, string Text);
}

// ── Factory ───────────────────────────────────────────────────────────────────

public sealed class QuotesApiFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryQuoteRepository _quotes = new();

    public void SeedQuote(CompetitiveQuote quote) => _quotes.Seed(quote);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:SeedOnStartup"] = "false",
                // Requis par le garde-fou de Program.cs hors Development.
                ["Jwt:Key"] = "TEST_ONLY_LONG_ENOUGH_SECRET_KEY_1234567890"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IQuoteRepository>();
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IBlazonRepository>();

            services.AddSingleton<IQuoteRepository>(_quotes);
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<IBlazonRepository>(new InMemoryBlazonRepository());

            // Auth remplacée par le handler de test (rôles pilotés par en-tête).
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public HttpClient CreateUserClient() => CreateClientWithRoles("User");

    public HttpClient CreateAdminClient() => CreateClientWithRoles("Admin");

    private HttpClient CreateClientWithRoles(params string[] roles)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(",", roles));
        return client;
    }
}

internal sealed class InMemoryQuoteRepository : IQuoteRepository
{
    private readonly List<CompetitiveQuote> _store = [];

    public void Seed(CompetitiveQuote quote) => _store.Add(quote);

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
