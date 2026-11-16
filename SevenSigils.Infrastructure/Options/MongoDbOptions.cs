namespace SevenSigils.Infrastructure.Options;

public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "sevensigils";
    public string BlazonCollection { get; set; } = "blazons";
    public string UserCollection { get; set; } = "users";
    public string QuoteCollection { get; set; } = "quotes";
    public string TierTitleCollection { get; set; } = "tierTitles";
    public bool SeedOnStartup { get; set; } = true;

    /// <summary>
    /// Comportement du seed au démarrage (si <see cref="SeedOnStartup"/>) :
    /// <see cref="SeedMode.IfEmpty"/> par défaut (dev : les éditions faites via la
    /// citadel sont préservées), <see cref="SeedMode.Replace"/> en prod (le dépôt
    /// Git est la source de vérité, la base est resynchronisée à chaque démarrage).
    /// </summary>
    public SeedMode SeedMode { get; set; } = SeedMode.IfEmpty;
}

public enum SeedMode
{
    /// <summary>Ne remplit une collection que si elle est vide.</summary>
    IfEmpty,

    /// <summary>Remplace le contenu des collections par les données de référence.</summary>
    Replace
}
