using SevenSigils.Domain.Models;

namespace SevenSigils.Domain.Abstractions;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}