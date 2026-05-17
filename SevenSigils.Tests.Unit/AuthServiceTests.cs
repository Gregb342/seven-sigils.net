using FluentAssertions;
using SevenSigils.Application.Auth;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Tests.Unit;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldCreateUser_AndReturnToken()
    {
        var repository = new FakeUserRepository();
        var sut = BuildSut(repository);

        var result = await sut.RegisterAsync(new RegisterUserCommand("User@Test.dev", "Password123!"));

        result.Email.Should().Be("user@test.dev");
        result.AccessToken.Should().Be("token:user@test.dev");
        result.Roles.Should().ContainSingle("User");
        repository.StoredUser.Should().NotBeNull();
        repository.StoredUser!.PasswordHash.Should().Be("hashed:Password123!");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var repository = new FakeUserRepository(
            new ApplicationUser("1", "user@test.dev", "hashed:Password123!", ["User"]));
        var sut = BuildSut(repository);

        var act = () => sut.RegisterAsync(new RegisterUserCommand(" user@test.dev ", "Password123!"));

        await act.Should().ThrowAsync<DuplicateEmailException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var repository = new FakeUserRepository(
            new ApplicationUser("1", "user@test.dev", "hashed:Password123!", ["User"]));
        var sut = BuildSut(repository);

        var result = await sut.LoginAsync(new LoginUserCommand("User@Test.dev", "Password123!"));

        result.AccessToken.Should().Be("token:user@test.dev");
        result.Email.Should().Be("user@test.dev");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var sut = BuildSut(new FakeUserRepository());

        var act = () => sut.LoginAsync(new LoginUserCommand("missing@test.dev", "Password123!"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsInvalid()
    {
        var repository = new FakeUserRepository(
            new ApplicationUser("1", "user@test.dev", "hashed:Password123!", ["User"]));
        var sut = BuildSut(repository);

        var act = () => sut.LoginAsync(new LoginUserCommand("user@test.dev", "WrongPassword!"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    private static AuthService BuildSut(FakeUserRepository repository) =>
        new(repository, new FakePasswordHasher(), new FakeAccessTokenGenerator());

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

    private sealed class FakeAccessTokenGenerator : IAccessTokenGenerator
    {
        public string Generate(ApplicationUser user) => $"token:{user.Email}";
    }
}