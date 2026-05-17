using SevenSigils.Domain.Models;

namespace SevenSigils.Domain.Abstractions;

public interface IAccessTokenGenerator
{
    string Generate(ApplicationUser user);
}