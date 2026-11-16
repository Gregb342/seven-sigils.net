using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SevenSigils.Tests.Integration;

// Production : Admin:Enabled=false. Les routes du back-office doivent avoir
// disparu (404, pas 401 : la surface d'attaque est retirée, pas seulement
// protégée), et l'API doit démarrer sans clé JWT ni compte admin.
public sealed class BackOfficeDisabledApiTests : IClassFixture<BackOfficeDisabledApiFactory>
{
    private readonly BackOfficeDisabledApiFactory _factory;

    public BackOfficeDisabledApiTests(BackOfficeDisabledApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_ShouldReturn404_WhenBackOfficeDisabled()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "admin@test.dev",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/api/v1/admin/blazons")]
    [InlineData("/api/v1/admin/blazons/export")]
    [InlineData("/api/v1/admin/quotes")]
    public async Task AdminRoutes_ShouldReturn404_WhenBackOfficeDisabled(string path)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(path);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AdminWrite_ShouldReturn404_WhenBackOfficeDisabled()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync("/api/v1/admin/blazons/stark");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PublicEndpoints_ShouldStillWork_WhenBackOfficeDisabled()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/version");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public sealed class BackOfficeDisabledApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Environnement hors Development et volontairement SANS Jwt:Key : le
        // fail-fast sur la clé ne doit pas s'appliquer quand le back-office est absent.
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:SeedOnStartup"] = "false",
                ["Admin:Enabled"] = "false"
            });
        });
    }
}
