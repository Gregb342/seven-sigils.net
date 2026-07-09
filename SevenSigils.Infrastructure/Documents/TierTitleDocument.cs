using MongoDB.Bson.Serialization.Attributes;

namespace SevenSigils.Infrastructure.Documents;

internal sealed class TierTitleDocument
{
    /// <summary>Le palier sert d'identifiant : un seul titre par palier.</summary>
    [BsonId]
    public string Tier { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;
}
