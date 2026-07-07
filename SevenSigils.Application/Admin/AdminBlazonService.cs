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

    public async Task<BlazonExport> ExportAsync(CancellationToken cancellationToken = default)
    {
        const int pageSize = 200;
        var all = new List<Blazon>();
        var page = 1;

        for (;;)
        {
            var (items, totalCount) = await _repository.GetAllAsync(page, pageSize, cancellationToken);
            all.AddRange(items);
            if (items.Count == 0 || all.Count >= totalCount)
                break;
            page++;
        }

        var easyModeSlugs = all
            .Where(b => b.IncludeInEasy)
            .Select(b => b.FamilySlug)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();

        var entries = all
            .OrderBy(b => b.FamilySlug, StringComparer.Ordinal)
            .ToDictionary(
                b => b.FamilySlug,
                b => new BlazonExportEntry(
                    Label: b.FamilyLabel,
                    DisplayName: b.DisplayName,
                    Kind: b.Kind,
                    VariantOf: b.VariantOf,
                    IncludeInHard: b.IncludeInHard,
                    HousePageUrl: b.HousePageUrl,
                    Hints: b.Hints,
                    Attribution: b.Attribution));

        return new BlazonExport(easyModeSlugs, entries);
    }
}
