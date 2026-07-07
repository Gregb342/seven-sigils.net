using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;
using SevenSigils.Infrastructure.Options;
using SevenSigils.Infrastructure.Seeding;

namespace SevenSigils.Tests.Unit;

public sealed class AdminUserSeederTests
{
    private const string StrongPassword = "CorrectHorseBattery1";

    [Fact]
    public async Task SeedAsync_ShouldCreateAdmin_WhenConfiguredAndAbsent()
    {
        var repository = new FakeUserRepository();
        var sut = BuildSut(repository, email: "Admin@Test.dev", password: StrongPassword);

        await sut.SeedAsync();

        repository.StoredUser.Should().NotBeNull();
        repository.StoredUser!.Email.Should().Be("admin@test.dev");
        repository.StoredUser.PasswordHash.Should().Be($"hashed:{StrongPassword}");
        repository.StoredUser.Roles.Should().ContainSingle().Which.Should().Be("Admin");
    }

    [Fact]
    public async Task SeedAsync_ShouldDoNothing_WhenNotConfigured()
    {
        var repository = new FakeUserRepository();
        var sut = BuildSut(repository, email: null, password: null);

        await sut.SeedAsync();

        repository.StoredUser.Should().BeNull();
    }

    [Theory]
    [InlineData("admin@test.dev", null)]
    [InlineData(null, StrongPassword)]
    public async Task SeedAsync_ShouldThrow_WhenPartiallyConfigured(string? email, string? password)
    {
        var sut = BuildSut(new FakeUserRepository(), email, password);

        var act = () => sut.SeedAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*misconfigured*");
    }

    [Fact]
    public async Task SeedAsync_ShouldThrow_WhenPasswordIsTooShort()
    {
        var sut = BuildSut(new FakeUserRepository(), "admin@test.dev", "short");

        var act = () => sut.SeedAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*at least*characters*");
    }

    [Fact]
    public async Task SeedAsync_ShouldThrow_WhenEmailIsInvalid()
    {
        var sut = BuildSut(new FakeUserRepository(), "not-an-email", StrongPassword);

        var act = () => sut.SeedAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*valid email*");
    }

    [Fact]
    public async Task SeedAsync_ShouldNotOverwrite_WhenAdminAlreadyExists()
    {
        var existing = new ApplicationUser("1", "admin@test.dev", "hashed:OldPasswordChanged1", ["Admin"]);
        var repository = new FakeUserRepository(existing);
        var sut = BuildSut(repository, "admin@test.dev", StrongPassword);

        await sut.SeedAsync();

        // Idempotence : le compte existant (mot de passe changé depuis) ne doit pas être écrasé.
        repository.StoredUser.Should().BeNull();
    }

    private static AdminUserSeeder BuildSut(FakeUserRepository repository, string? email, string? password) =>
        new(
            repository,
            new FakePasswordHasher(),
            Options.Create(new AdminSeedOptions { Email = email, Password = password }),
            NullLogger<AdminUserSeeder>.Instance);

    private sealed class FakeUserRepository(params ApplicationUser[] users) : IUserRepository
    {
        private readonly List<ApplicationUser> _users = [.. users];

        public ApplicationUser? StoredUser { get; private set; }

        public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_users.SingleOrDefault(x => x.Email == email));
        }

        public Task CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _users.Add(user);
            StoredUser = user;
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"hashed:{password}";

        public bool Verify(string password, string passwordHash) => passwordHash == Hash(password);
    }
}
