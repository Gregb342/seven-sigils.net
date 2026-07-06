using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SevenSigils.Application.Auth;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Tests.Integration;

public sealed class AuthApiTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory _factory;

    public AuthApiTests(AuthApiFactory factory)
    {
        _factory = factory;
    }

    // L'inscription publique n'existe plus : l'endpoint doit avoir disparu.
    [Fact]
    public async Task Register_ShouldReturn404_EndpointRemoved()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "user@test.dev",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Plus de register : l'utilisateur est seedé directement (comme le fera le seeder admin).
        _factory.SeedUser(new ApplicationUser("1", "login@test.dev", "hashed:Password123!", ["Admin"]));
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "login@test.dev",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseBody>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("login@test.dev");
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Roles.Should().ContainSingle().Which.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "missing@test.dev",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record AuthResponseBody(string AccessToken, string Email, string[] Roles);
}

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    private readonly InMemoryUserRepository _users = new();

    public void SeedUser(ApplicationUser user) => _users.Seed(user);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:SeedOnStartup"] = "false",
                ["Jwt:Key"] = "TEST_ONLY_LONG_ENOUGH_SECRET_KEY_1234567890",
                ["Jwt:Issuer"] = "SevenSigils.Tests",
                ["Jwt:Audience"] = "SevenSigils.Tests"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IPasswordHasher>();
            services.RemoveAll<IAccessTokenGenerator>();
            services.RemoveAll<IAuthService>();

            services.AddSingleton<IUserRepository>(_users);
            services.AddSingleton<IPasswordHasher, FakePasswordHasher>();
            services.AddSingleton<IAccessTokenGenerator, FakeAccessTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();
        });
    }
}

internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, ApplicationUser> _users = new(StringComparer.OrdinalIgnoreCase);

    public void Seed(ApplicationUser user) => _users[user.Email] = user;

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _users.TryGetValue(email, out var user);
        return Task.FromResult(user);
    }

    public Task CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _users[user.Email] = user;
        return Task.CompletedTask;
    }
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => $"hashed:{password}";

    public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
}

internal sealed class FakeAccessTokenGenerator : IAccessTokenGenerator
{
    public string Generate(ApplicationUser user) => $"token:{user.Email}";
}