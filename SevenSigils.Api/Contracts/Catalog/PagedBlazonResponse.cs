namespace SevenSigils.Api.Contracts.Catalog;

public sealed record PagedBlazonResponse(
    IReadOnlyList<BlazonResponse> Items,
    long TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
