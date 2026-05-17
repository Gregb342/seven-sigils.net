using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SevenSigils.Api.Contracts.Catalog;
using SevenSigils.Application.Catalog;

namespace SevenSigils.Api.Controllers;

[ApiController]
[Route("api/v1/catalog")]
[Authorize]
public sealed class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    [ProducesResponseType<PagedBlazonResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize is < 1 or > 100)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid pagination parameters",
                Detail = "page must be ≥ 1 and pageSize must be between 1 and 100."
            });
        }

        var result = await _catalogService.GetPageAsync(page, pageSize, cancellationToken);

        return Ok(new PagedBlazonResponse(
            Items: result.Items.Select(ToResponse).ToList(),
            TotalCount: result.TotalCount,
            Page: result.Page,
            PageSize: result.PageSize,
            TotalPages: result.TotalPages));
    }

    [HttpGet("{slug}")]
    [ProducesResponseType<BlazonResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken = default)
    {
        var blazon = await _catalogService.GetBySlugAsync(slug, cancellationToken);

        if (blazon is null)
            return NotFound();

        return Ok(ToResponse(blazon));
    }

    private static BlazonResponse ToResponse(BlazonDto dto) => new(
        Id: dto.Id,
        FamilySlug: dto.FamilySlug,
        FamilyLabel: dto.FamilyLabel,
        DisplayName: dto.DisplayName,
        HousePageUrl: dto.HousePageUrl,
        Kind: dto.Kind,
        VariantOf: dto.VariantOf,
        IncludeInEasy: dto.IncludeInEasy,
        IncludeInHard: dto.IncludeInHard,
        Hints: dto.Hints.Select(h => new HouseHintResponse(h.Title, h.Value)).ToList(),
        Attribution: new AttributionResponse(
            dto.Attribution.Author,
            dto.Attribution.SourcePageUrl,
            dto.Attribution.LicenseLabel,
            dto.Attribution.LicenseUrl,
            dto.Attribution.Notes));
}
