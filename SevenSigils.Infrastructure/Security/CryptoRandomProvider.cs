using SevenSigils.Domain.Abstractions;

namespace SevenSigils.Infrastructure.Security;

public sealed class CryptoRandomProvider : IRandomProvider
{
    public double NextDouble() => Random.Shared.NextDouble();
}
