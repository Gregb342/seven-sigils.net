using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Tests.Integration;

// ── Tests — Catalog ───────────────────────────────────────────────────────────

public sealed class CatalogApiTests : IClassFixture<CatalogApiFactory>
{
    private readonly CatalogApiFactory _factory;

    public CatalogApiTests(CatalogApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_ShouldReturn401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/catalog");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_ShouldReturnPagedResult_WhenAuthenticated()
    {
        _factory.SeedBlazon(CatalogBlazon("stark", "Stark"));
        _factory.SeedBlazon(CatalogBlazon("lannister", "Lannister"));
        var client = _factory.CreateUserClient();

        var response = await client.GetAsync("/api/v1/catalog?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<PagedBody>();
        body.Should().NotBeNull();
        body!.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        body.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturn400_WhenPaginationIsInvalid()
    {
        var client = _factory.CreateUserClient();

        var response = await client.GetAsync("/api/v1/catalog?page=0&pageSize=0");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetBySlug_ShouldReturnBlazon_WhenItExists()
    {
        _factory.SeedBlazon(CatalogBlazon("tyrell", "Tyrell"));
        var client = _factory.CreateUserClient();

        var response = await client.GetAsync("/api/v1/catalog/tyrell");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<BlazonBody>();
        body!.FamilySlug.Should().Be("tyrell");
    }

    [Fact]
    public async Task GetBySlug_ShouldReturn404_WhenBlazonDoesNotExist()
    {
        var client = _factory.CreateUserClient();

        var response = await client.GetAsync("/api/v1/catalog/unknown-house");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static Blazon CatalogBlazon(string slug, string label) =>
        new(slug, slug, label, null, "https://example.test", null, null,
            true, true, [],
            new Attribution(null, "https://example.test",
                "CC BY-SA 4.0", "https://creativecommons.org/licenses/by-sa/4.0/", null));

    private sealed record PagedBody(List<BlazonBody> Items, long TotalCount, int Page, int PageSize, int TotalPages);
    private sealed record BlazonBody(string Id, string FamilySlug, string FamilyLabel);
}

// ── Tests — Admin ─────────────────────────────────────────────────────────────

public sealed class AdminApiTests : IClassFixture<CatalogApiFactory>
{
    private readonly CatalogApiFactory _factory;

    public AdminApiTests(CatalogApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Create_ShouldReturn401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/blazons", SampleCreatePayload("baratheon"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_ShouldReturn403_WhenUserRoleIsInsufficient()
    {
        var client = _factory.CreateUserClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/blazons", SampleCreatePayload("baratheon"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WhenAdminCreatesNewBlazon()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/blazons", SampleCreatePayload("martell-new"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_ShouldReturn409_WhenSlugAlreadyExists()
    {
        _factory.SeedBlazon(AdminBlazon("tully"));
        var client = _factory.CreateAdminClient();

        var response = await client.PostAsJsonAsync("/api/v1/admin/blazons", SampleCreatePayload("tully"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenAdminDeletesExistingBlazon()
    {
        _factory.SeedBlazon(AdminBlazon("mormont-to-delete"));
        var client = _factory.CreateAdminClient();

        var response = await client.DeleteAsync("/api/v1/admin/blazons/mormont-to-delete");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenBlazonDoesNotExist()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.DeleteAsync("/api/v1/admin/blazons/never-existed");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturn200_WhenAdminUpdatesExistingBlazon()
    {
        _factory.SeedBlazon(AdminBlazon("frey-to-update"));
        var client = _factory.CreateAdminClient();
        var payload = SampleUpdatePayload("Frey Updated");

        var response = await client.PutAsJsonAsync("/api/v1/admin/blazons/frey-to-update", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ShouldReturn404_WhenBlazonDoesNotExist()
    {
        var client = _factory.CreateAdminClient();

        var response = await client.PutAsJsonAsync("/api/v1/admin/blazons/ghost", SampleUpdatePayload("Ghost"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static Blazon AdminBlazon(string slug) =>
        new(slug, slug, slug, null, "https://example.test", null, null,
            true, true, [],
            new Attribution(null, "https://example.test",
                "CC BY-SA 4.0", "https://creativecommons.org/licenses/by-sa/4.0/", null));

    private static object SampleCreatePayload(string slug) => new
    {
        familySlug = slug,
        familyLabel = char.ToUpperInvariant(slug[0]) + slug[1..],
        housePageUrl = "https://example.test",
        includeInEasy = true,
        includeInHard = true,
        hints = Array.Empty<object>(),
        attribution = new
        {
            sourcePageUrl = "https://example.test",
            licenseLabel = "CC BY-SA 4.0",
            licenseUrl = "https://creativecommons.org/licenses/by-sa/4.0/"
        }
    };

    private static object SampleUpdatePayload(string label) => new
    {
        familyLabel = label,
        housePageUrl = "https://example.test",
        includeInEasy = true,
        includeInHard = true,
        hints = Array.Empty<object>(),
        attribution = new
        {
            sourcePageUrl = "https://example.test",
            licenseLabel = "CC BY-SA 4.0",
            licenseUrl = "https://creativecommons.org/licenses/by-sa/4.0/"
        }
    };
}

// ── Factory ───────────────────────────────────────────────────────────────────

public sealed class CatalogApiFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryBlazonRepository _blazons = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:SeedOnStartup"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IBlazonRepository>();

            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<IBlazonRepository>(_blazons);

            // Replace JWT authentication with a simple test handler
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public void SeedBlazon(Blazon blazon) => _blazons.Seed(blazon);

    public HttpClient CreateUserClient() => CreateClientWithRoles("User");

    public HttpClient CreateAdminClient() => CreateClientWithRoles("Admin");

    private HttpClient CreateClientWithRoles(params string[] roles)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(",", roles));
        return client;
    }
}

// ── Test authentication handler ───────────────────────────────────────────────

internal sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";
    public const string RolesHeader = "X-Test-Roles";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(RolesHeader, out var rolesValue))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = rolesValue.ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(r => new Claim(ClaimTypes.Role, r))
            .Append(new Claim(ClaimTypes.NameIdentifier, "test-user"))
            .ToList();

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// ── In-memory repository ──────────────────────────────────────────────────────

internal sealed class InMemoryBlazonRepository : IBlazonRepository
{
    private readonly List<Blazon> _store = [];

    public void Seed(Blazon blazon) => _store.Add(blazon);

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
