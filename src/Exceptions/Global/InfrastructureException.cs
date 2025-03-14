using Org.BouncyCastle.Asn1.Pkcs;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions.Global;

public abstract class InfrastructureException : Exception
{
    public InfrastructureException() : base()
    {

    }

    public InfrastructureException(string? message) : base(message)
    {

    }

    public InfrastructureException(string? message, Exception? exception) : base(message, exception)
    {

    }

    [DoesNotReturn]
    public abstract void ThrowNoStackTrace(string message);

    [DoesNotReturn]
    public abstract void ThrowWithStackTrace(Exception exception);
}
