using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SevenSigils.Api.Contracts.Quotes;
using SevenSigils.Application.Quotes;

namespace SevenSigils.Api.Controllers;

[ApiController]
[Route("api/v1/admin/quotes")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminQuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public AdminQuotesController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpPost]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] SaveQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await _quoteService.CreateAsync(request.Tier, request.Text, cancellationToken);
        return Created(
            $"/api/v1/quotes/{created.Id}",
            new QuoteResponse(created.Id, created.Tier, created.Text));
    }

    [HttpPut("{id}")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] string id,
        [FromBody] SaveQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _quoteService.UpdateAsync(id, request.Tier, request.Text, cancellationToken);
            return Ok(new QuoteResponse(updated.Id, updated.Tier, updated.Text));
        }
        catch (QuoteNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _quoteService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (QuoteNotFoundException)
        {
            return NotFound();
        }
    }
}
