namespace SevenSigils.Application.Auth;

public sealed class InvalidCredentialsException : InvalidOperationException
{
    public InvalidCredentialsException()
        : base("Invalid email or password.")
    {
    }
}