namespace SevenSigils.Application.Auth;

public sealed class DuplicateEmailException : InvalidOperationException
{
    public DuplicateEmailException(string email)
        : base($"A user with email '{email}' already exists.")
    {
    }
}