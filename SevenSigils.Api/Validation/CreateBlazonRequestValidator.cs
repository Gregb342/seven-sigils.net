using FluentValidation;
using SevenSigils.Api.Contracts.Admin;

namespace SevenSigils.Api.Validation;

public sealed class CreateBlazonRequestValidator : AbstractValidator<CreateBlazonRequest>
{
    public CreateBlazonRequestValidator()
    {
        RuleFor(x => x.FamilySlug)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[a-z0-9\-]+$").WithMessage("Slug must contain only lowercase letters, digits and hyphens.");

        RuleFor(x => x.FamilyLabel)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.HousePageUrl)
            .NotEmpty()
            .MaximumLength(500)
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _)).WithMessage("HousePageUrl must be a valid absolute URL.");

        RuleFor(x => x.Attribution).NotNull();
        RuleFor(x => x.Attribution.SourcePageUrl)
            .NotEmpty()
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _)).WithMessage("Attribution.SourcePageUrl must be a valid absolute URL.")
            .When(x => x.Attribution is not null);

        RuleFor(x => x.Attribution.LicenseLabel)
            .NotEmpty()
            .When(x => x.Attribution is not null);

        RuleFor(x => x.Attribution.LicenseUrl)
            .NotEmpty()
            .Must(u => Uri.TryCreate(u, UriKind.Absolute, out _)).WithMessage("Attribution.LicenseUrl must be a valid absolute URL.")
            .When(x => x.Attribution is not null);
    }
}
