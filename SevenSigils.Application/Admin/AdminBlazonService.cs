using SevenSigils.Application.Catalog;
using SevenSigils.Domain.Abstractions;
using SevenSigils.Domain.Models;

namespace SevenSigils.Application.Admin;

public sealed class AdminBlazonService : IAdminBlazonService
{
    private readonly IBlazonRepository _repository;

    public AdminBlazonService(IBlazonRepository repository)
    {
        _repository = repository;
    }

    public async Task<BlazonDto> CreateAsync(CreateBlazonCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetBySlugAsync(command.FamilySlug, cancellationToken);
        if (existing is not null)
            throw new SlugAlreadyExistsException(command.FamilySlug);

        var blazon = new Blazon(
            Id: Guid.NewGuid().ToString("N"),
            FamilySlug: command.FamilySlug,
            FamilyLabel: command.FamilyLabel,
            DisplayName: command.DisplayName,
            HousePageUrl: command.HousePageUrl,
            Kind: command.Kind,
            VariantOf: command.VariantOf,
            IncludeInEasy: command.IncludeInEasy,
            IncludeInHard: command.IncludeInHard,
            Hints: command.Hints,
            Attribution: command.Attribution);

        var created = await _repository.CreateAsync(blazon, cancellationToken);
        return BlazonDto.From(created);
    }

    public async Task<BlazonDto> UpdateAsync(string familySlug, UpdateBlazonCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetBySlugAsync(familySlug, cancellationToken);
        if (existing is null)
            throw new BlazonNotFoundException(familySlug);

        var updated = existing with
        {
            FamilyLabel = command.FamilyLabel,
            DisplayName = command.DisplayName,
            HousePageUrl = command.HousePageUrl,
            Kind = command.Kind,
            VariantOf = command.VariantOf,
            IncludeInEasy = command.IncludeInEasy,
            IncludeInHard = command.IncludeInHard,
            Hints = command.Hints,
            Attribution = command.Attribution
        };

        var result = await _repository.UpdateAsync(updated, cancellationToken);
        return BlazonDto.From(result!);
    }

    public async Task DeleteAsync(string familySlug, CancellationToken cancellationToken = default)
    {
        var deleted = await _repository.DeleteAsync(familySlug, cancellationToken);
        if (!deleted)
            throw new BlazonNotFoundException(familySlug);
    }
}
