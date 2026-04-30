namespace SevenSigils.Domain.Models;

public sealed record Question(
    Blazon Blazon,
    IReadOnlyList<string> Options,
    string CorrectOption);
