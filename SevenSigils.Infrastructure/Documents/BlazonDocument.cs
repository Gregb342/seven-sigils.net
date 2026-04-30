using MongoDB.Bson.Serialization.Attributes;

namespace SevenSigils.Infrastructure.Documents;

internal sealed class BlazonDocument
{
    [BsonId]
    public string Id { get; set; } = string.Empty;

    [BsonElement("familySlug")]
    public string FamilySlug { get; set; } = string.Empty;

    [BsonElement("familyLabel")]
    public string FamilyLabel { get; set; } = string.Empty;

    [BsonElement("displayName")]
    public string? DisplayName { get; set; }

    [BsonElement("housePageUrl")]
    public string HousePageUrl { get; set; } = string.Empty;

    [BsonElement("kind")]
    public string? Kind { get; set; }

    [BsonElement("variantOf")]
    public string? VariantOf { get; set; }

    [BsonElement("includeInHard")]
    public bool IncludeInHard { get; set; }

    [BsonElement("isEasy")]
    public bool IsEasy { get; set; }

    [BsonElement("hints")]
    public List<HouseHintDocument> Hints { get; set; } = new();

    [BsonElement("attribution")]
    public AttributionDocument Attribution { get; set; } = new();
}

internal sealed class HouseHintDocument
{
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("value")]
    public string Value { get; set; } = string.Empty;
}

internal sealed class AttributionDocument
{
    [BsonElement("author")]
    public string? Author { get; set; }

    [BsonElement("sourcePageUrl")]
    public string SourcePageUrl { get; set; } = string.Empty;

    [BsonElement("licenseLabel")]
    public string LicenseLabel { get; set; } = string.Empty;

    [BsonElement("licenseUrl")]
    public string LicenseUrl { get; set; } = string.Empty;

    [BsonElement("notes")]
    public string? Notes { get; set; }
}
