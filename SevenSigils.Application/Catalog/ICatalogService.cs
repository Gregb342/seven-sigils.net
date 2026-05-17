namespace SevenSigils.Application.Catalog;

public interface ICatalogService
{
    Task<PagedResult<BlazonDto>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<BlazonDto?> GetBySlugAsync(string familySlug, CancellationToken cancellationToken = default);
}
