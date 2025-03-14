using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions;

public class CryptographyException : InfrastructureException
{
    public CryptographyException() : base()
    {

    }

    public CryptographyException(string? message) : base(message)
    {

    }

    public CryptographyException(string? message, Exception? exception) : base(message, exception)
    {

    }

    [DoesNotReturn]
    public override void Throw(string message)
        => throw new CryptographyException(message);

    public override void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new CryptographyException(message);
    }

    public override void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new CryptographyException(message);
    }
}
