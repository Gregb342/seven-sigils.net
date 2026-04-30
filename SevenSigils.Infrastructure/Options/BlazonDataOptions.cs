namespace SevenSigils.Infrastructure.Options;

public sealed class BlazonDataOptions
{
    public const string SectionName = "BlazonData";

    public string JsonPath { get; set; } = "data/blazonDb.json";
}
