using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SevenSigils.Application.Services;
using SevenSigils.Domain.Models;

namespace SevenSigils.Api.Controllers;

[ApiController]
[Route("api/v1/quiz")]
public sealed class QuizController : ControllerBase
{
    private readonly IQuizQuestionService _quizQuestionService;

    public QuizController(IQuizQuestionService quizQuestionService)
    {
        _quizQuestionService = quizQuestionService;
    }

    [HttpPost("question")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateQuestion([FromBody] QuestionRequest request, CancellationToken cancellationToken)
    {
        var difficulty = request.Difficulty?.Equals("hard", StringComparison.OrdinalIgnoreCase) == true
            ? Difficulty.Hard
            : Difficulty.Easy;

        try
        {
            var question = await _quizQuestionService.CreateQuestionAsync(
                difficulty,
                request.ExcludedIds?.ToArray() ?? Array.Empty<string>(),
                cancellationToken);

            return Ok(question);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    public sealed class QuestionRequest
    {
        public string? Difficulty { get; set; }
        public List<string>? ExcludedIds { get; set; }
    }
}

