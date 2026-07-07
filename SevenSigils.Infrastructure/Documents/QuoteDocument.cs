using MongoDB.Bson.Serialization.Attributes;

namespace SevenSigils.Infrastructure.Documents;

internal sealed class QuoteDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    [BsonElement("tier")]
    public string Tier { get; set; } = string.Empty;

    [BsonElement("text")]
    public string Text { get; set; } = string.Empty;
}
