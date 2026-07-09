using Microsoft.Extensions.Logging;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Infrastructure.Seeding;

/// <summary>
/// Seed initial des citations de fin de partie compétitive : si la collection est
/// vide, elle est remplie avec les mêmes textes que le fallback embarqué dans le
/// frontend (resources/quotes.ts). Ne touche jamais une collection non vide —
/// les éditions faites via la citadel sont préservées (Mongo est canonique).
/// </summary>
public sealed class QuoteSeeder
{
    private static readonly (string Tier, string Text)[] Defaults =
    [
        (QuoteTiers.Legendary, "Le savoir est une arme. Tu es redoutablement armé."),
        (QuoteTiers.Legendary, "Un esprit a besoin de livres comme une épée a besoin d’une pierre à aiguiser."),
        (QuoteTiers.Legendary, "Les archives de la Citadelle se souviendront de ton nom."),
        (QuoteTiers.Strong, "Un Lannister paie toujours ses dettes — et toi, tu honores tes blasons."),
        (QuoteTiers.Strong, "Le Nord se souvient. De toi aussi, désormais."),
        (QuoteTiers.Strong, "Quand on joue au jeu des blasons, on gagne ou on révise."),
        (QuoteTiers.Average, "Ton tour de garde ne fait que commencer."),
        (QuoteTiers.Average, "La Garde a besoin d’hommes qui persévèrent. Rejoue."),
        (QuoteTiers.Average, "Ni gloire, ni honte : le Mur tient, et toi aussi."),
        (QuoteTiers.Grim, "Vous ne savez rien… mais ça se soigne, à l’encyclopédie."),
        (QuoteTiers.Grim, "La nuit est sombre et pleine d’erreurs."),
        (QuoteTiers.Grim, "L’hiver vient. Il est même déjà là, visiblement."),
    ];

    private readonly IQuoteRepository _repository;
    private readonly ILogger<QuoteSeeder> _logger;

    public QuoteSeeder(IQuoteRepository repository, ILogger<QuoteSeeder> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    private static readonly (string Tier, string Title)[] DefaultTitles =
    [
        (QuoteTiers.Legendary, "Mestre de la Citadelle"),
        (QuoteTiers.Strong, "Main du Roi"),
        (QuoteTiers.Average, "Frère juré de la Garde de Nuit"),
        (QuoteTiers.Grim, "Marcheur d’hiver"),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedQuotesAsync(cancellationToken);
        await SeedTitlesAsync(cancellationToken);
    }

    private async Task SeedQuotesAsync(CancellationToken cancellationToken)
    {
        var existing = await _repository.GetAllAsync(cancellationToken);
        if (existing.Count > 0)
        {
            _logger.LogInformation("Quote collection already contains {Count} documents — skipping seed.", existing.Count);
            return;
        }

        foreach (var (tier, text) in Defaults)
        {
            await _repository.CreateAsync(
                new CompetitiveQuote(Guid.NewGuid().ToString("N"), tier, text),
                cancellationToken);
        }

        _logger.LogInformation("Seeded {Count} competitive quotes into MongoDB.", Defaults.Length);
    }

    private async Task SeedTitlesAsync(CancellationToken cancellationToken)
    {
        var existing = await _repository.GetTierTitlesAsync(cancellationToken);
        if (existing.Count > 0)
        {
            _logger.LogInformation("Tier titles already present ({Count}) — skipping seed.", existing.Count);
            return;
        }

        foreach (var (tier, title) in DefaultTitles)
        {
            await _repository.UpsertTierTitleAsync(tier, title, cancellationToken);
        }

        _logger.LogInformation("Seeded {Count} tier titles into MongoDB.", DefaultTitles.Length);
    }
}
