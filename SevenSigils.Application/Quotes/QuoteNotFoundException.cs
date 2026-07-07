namespace SevenSigils.Application.Quotes;

public sealed class QuoteNotFoundException(string id)
    : Exception($"No quote found with id '{id}'.");
