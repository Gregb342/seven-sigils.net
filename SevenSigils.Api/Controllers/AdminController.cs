using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SevenSigils.Api.Contracts.Admin;
using SevenSigils.Api.Contracts.Catalog;
using SevenSigils.Application.Admin;
using SevenSigils.Application.Catalog;
using SevenSigils.Domain.Models;

namespace SevenSigils.Api.Controllers;

[ApiController]
[Route("api/v1/admin/blazons")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminController : ControllerBase
{
    private readonly IAdminBlazonService _adminService;

    public AdminController(IAdminBlazonService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost]
    [ProducesResponseType<BlazonResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBlazonRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _adminService.CreateAsync(ToCommand(request), cancellationToken);
            return CreatedAtAction(
                nameof(CatalogController.GetBySlug),
                "Catalog",
                new { slug = result.FamilySlug },
                ToResponse(result));
        }
        catch (SlugAlreadyExistsException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Slug already exists",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPut("{slug}")]
    [ProducesResponseType<BlazonResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] string slug,
        [FromBody] UpdateBlazonRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _adminService.UpdateAsync(slug, ToCommand(request), cancellationToken);
            return Ok(ToResponse(result));
        }
        catch (BlazonNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{slug}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] string slug,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _adminService.DeleteAsync(slug, cancellationToken);
            return NoContent();
        }
        catch (BlazonNotFoundException)
        {
            return NotFound();
        }
    }

    private static CreateBlazonCommand ToCommand(CreateBlazonRequest request) => new(
        FamilySlug: request.FamilySlug,
        FamilyLabel: request.FamilyLabel,
        DisplayName: request.DisplayName,
        HousePageUrl: request.HousePageUrl,
        Kind: request.Kind,
        VariantOf: request.VariantOf,
        IncludeInEasy: request.IncludeInEasy,
        IncludeInHard: request.IncludeInHard,
        Hints: request.Hints.Select(h => new HouseHint(h.Title, h.Value)).ToList(),
        Attribution: new Attribution(
            request.Attribution.Author,
            request.Attribution.SourcePageUrl,
            request.Attribution.LicenseLabel,
            request.Attribution.LicenseUrl,
            request.Attribution.Notes ?? string.Empty));

    private static UpdateBlazonCommand ToCommand(UpdateBlazonRequest request) => new(
        FamilyLabel: request.FamilyLabel,
        DisplayName: request.DisplayName,
        HousePageUrl: request.HousePageUrl,
        Kind: request.Kind,
        VariantOf: request.VariantOf,
        IncludeInEasy: request.IncludeInEasy,
        IncludeInHard: request.IncludeInHard,
        Hints: request.Hints.Select(h => new HouseHint(h.Title, h.Value)).ToList(),
        Attribution: new Attribution(
            request.Attribution.Author,
            request.Attribution.SourcePageUrl,
            request.Attribution.LicenseLabel,
            request.Attribution.LicenseUrl,
            request.Attribution.Notes ?? string.Empty));

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
