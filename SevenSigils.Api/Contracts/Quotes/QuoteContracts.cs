namespace SevenSigils.Api.Contracts.Quotes;

public sealed record QuoteResponse(string Id, string Tier, string Text);

/// <summary>Contenu complet des écrans de fin : titres de rang + citations, par palier.</summary>
public sealed record QuotesResponse(
    IReadOnlyDictionary<string, string> Titles,
    IReadOnlyList<QuoteResponse> Quotes);

public sealed record SaveQuoteRequest(string Tier, string Text);

public sealed record SaveTierTitleRequest(string Title);

public sealed record TierTitleResponse(string Tier, string Title);
