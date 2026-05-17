using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;
using SevenSigils.Infrastructure.Documents;
using SevenSigils.Infrastructure.Options;

namespace SevenSigils.Infrastructure.Repositories;

public sealed class MongoDbUserRepository : IUserRepository
{
    private readonly IMongoCollection<ApplicationUserDocument> _collection;

    public MongoDbUserRepository(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        var database = mongoClient.GetDatabase(options.Value.DatabaseName);
        _collection = database.GetCollection<ApplicationUserDocument>(options.Value.UserCollection);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var document = await _collection
            .Find(x => x.Email == email)
            .SingleOrDefaultAsync(cancellationToken);

        return document is null ? null : ToDomain(document);
    }

    public async Task CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(ToDocument(user), cancellationToken: cancellationToken);
    }

    private static ApplicationUser ToDomain(ApplicationUserDocument doc) => new(
        doc.Id,
        doc.Email,
        doc.PasswordHash,
        doc.Roles);

    private static ApplicationUserDocument ToDocument(ApplicationUser user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        PasswordHash = user.PasswordHash,
        Roles = [.. user.Roles]
    };
}