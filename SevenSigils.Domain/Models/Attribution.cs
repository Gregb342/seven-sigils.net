namespace SevenSigils.Domain.Models;

public sealed record Attribution(
    string? Author,
    string SourcePageUrl,
    string LicenseLabel,
    string LicenseUrl,
    string Notes);
