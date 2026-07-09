using FluentValidation;
using SevenSigils.Api.Contracts.Quotes;

namespace SevenSigils.Api.Validation;

public sealed class SaveTierTitleRequestValidator : AbstractValidator<SaveTierTitleRequest>
{
    public SaveTierTitleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);
    }
}
