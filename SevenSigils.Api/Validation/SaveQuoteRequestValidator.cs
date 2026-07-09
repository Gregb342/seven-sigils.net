using FluentValidation;
using SevenSigils.Api.Contracts.Quotes;
using SevenSigils.Domain.Models;

namespace SevenSigils.Api.Validation;

public sealed class SaveQuoteRequestValidator : AbstractValidator<SaveQuoteRequest>
{
    public SaveQuoteRequestValidator()
    {
        RuleFor(x => x.Tier)
            .NotEmpty()
            .Must(QuoteTiers.IsValid)
            .WithMessage($"Tier must be one of: {string.Join(", ", QuoteTiers.All)}.");

        RuleFor(x => x.Text)
            .NotEmpty()
            .MaximumLength(300);
    }
}
