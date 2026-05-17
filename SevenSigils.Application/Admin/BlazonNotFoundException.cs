namespace SevenSigils.Application.Admin;

public sealed class BlazonNotFoundException : Exception
{
    public BlazonNotFoundException(string familySlug)
        : base($"Blazon with slug '{familySlug}' was not found.") { }
}
