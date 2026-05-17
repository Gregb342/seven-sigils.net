using MongoDB.Bson.Serialization.Attributes;

namespace SevenSigils.Infrastructure.Documents;

internal sealed class ApplicationUserDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("roles")]
    public List<string> Roles { get; set; } = [];
}