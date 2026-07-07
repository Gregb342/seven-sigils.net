namespace SevenSigils.Api.Contracts.Quotes;

public sealed record QuoteResponse(string Id, string Tier, string Text);

public sealed record SaveQuoteRequest(string Tier, string Text);
