using SevenSigils.Domain.Abstractions;

namespace SevenSigils.Application.Catalog;

public sealed class CatalogService : ICatalogService
{
    private readonly IBlazonRepository _repository;

    public CatalogService(IBlazonRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<BlazonDto>> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetAllAsync(page, pageSize, cancellationToken);

        return new PagedResult<BlazonDto>(
            Items: items.Select(BlazonDto.From).ToList(),
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize);
    }

    public async Task<BlazonDto?> GetBySlugAsync(string familySlug, CancellationToken cancellationToken = default)
    {
        var blazon = await _repository.GetBySlugAsync(familySlug, cancellationToken);
        return blazon is null ? null : BlazonDto.From(blazon);
    }
}
