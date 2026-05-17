namespace SevenSigils.Application.Admin;

public sealed class SlugAlreadyExistsException : Exception
{
    public SlugAlreadyExistsException(string familySlug)
        : base($"A blazon with slug '{familySlug}' already exists.") { }
}
