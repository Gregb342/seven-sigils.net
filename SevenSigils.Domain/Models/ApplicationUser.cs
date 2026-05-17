namespace SevenSigils.Domain.Models;

public sealed record ApplicationUser(
    string Id,
    string Email,
    string PasswordHash,
    IReadOnlyList<string> Roles);