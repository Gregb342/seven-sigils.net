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

namespace SevenSigils.Tests.Integration;

// Chaîne complète : Program démarre → AdminUserSeeder crée le compte depuis la
// configuration → le login avec ces credentials délivre un token au rôle Admin.
public sealed class AdminSeederApiTests : IClassFixture<AdminSeederApiFactory>
{
    private readonly AdminSeederApiFactory _factory;

    public AdminSeederApiTests(AdminSeederApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ShouldSucceedWithAdminRole_WhenAdminWasSeededFromConfiguration()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = AdminSeederApiFactory.AdminEmail,
            password = AdminSeederApiFactory.AdminPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseBody>();
        body.Should().NotBeNull();
        body!.Email.Should().Be(AdminSeederApiFactory.AdminEmail);
        body.Roles.Should().ContainSingle().Which.Should().Be("Admin");
    }

    private sealed record AuthResponseBody(string AccessToken, string Email, string[] Roles);
}

public sealed class AdminSeederApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "maester@citadel.test";
    public const string AdminPassword = "AValyrianSteelPassword1";

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
                ["Jwt:Audience"] = "SevenSigils.Tests",
                ["Admin:Email"] = AdminEmail,
                ["Admin:Password"] = AdminPassword
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
