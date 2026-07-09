using Microsoft.AspNetCore.Mvc;
using SevenSigils.Api.Contracts.Quotes;
using SevenSigils.Application.Quotes;

namespace SevenSigils.Api.Controllers;

// Lecture publique : le frontend affiche les citations en fin de partie compétitive.
// Les écritures vivent dans AdminQuotesController (policy AdminOnly).
[ApiController]
[Route("api/v1/quotes")]
public sealed class QuotesController : ControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<QuoteResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var quotes = await _quoteService.GetAllAsync(cancellationToken);
        return Ok(quotes.Select(q => new QuoteResponse(q.Id, q.Tier, q.Text)).ToList());
    }
}
