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

    [Fact]
    public async Task Register_ShouldReturnToken_WhenPayloadIsValid()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "user@test.dev",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseBody>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("user@test.dev");
        body.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Roles.Should().ContainSingle("User");
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "duplicate@test.dev",
            password = "Password123!"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "duplicate@test.dev",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordIsWeak()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "weak@test.dev",
            password = "weak"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var client = _factory.CreateClient();

        await client.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = "login@test.dev",
            password = "Password123!"
        });

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "login@test.dev",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
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

            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
            services.AddSingleton<IPasswordHasher, FakePasswordHasher>();
            services.AddSingleton<IAccessTokenGenerator, FakeAccessTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();
        });
    }
}

internal sealed class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, ApplicationUser> _users = new(StringComparer.OrdinalIgnoreCase);

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