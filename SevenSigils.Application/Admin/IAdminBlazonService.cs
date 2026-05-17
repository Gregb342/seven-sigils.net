using SevenSigils.Application.Catalog;

namespace SevenSigils.Application.Admin;

public interface IAdminBlazonService
{
    Task<BlazonDto> CreateAsync(CreateBlazonCommand command, CancellationToken cancellationToken = default);

    Task<BlazonDto> UpdateAsync(string familySlug, UpdateBlazonCommand command, CancellationToken cancellationToken = default);

    Task DeleteAsync(string familySlug, CancellationToken cancellationToken = default);
}
