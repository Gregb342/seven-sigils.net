namespace SevenSigils.Infrastructure.Options;

public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "sevensigils";
    public string BlazonCollection { get; set; } = "blazons";
}
