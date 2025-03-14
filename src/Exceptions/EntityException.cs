using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;

namespace Infrastructure.Exceptions;

public class EntityException : InfrastructureException
{
    public EntityException() : base()
    {

    }

    public EntityException(string? message) : base(message)
    {

    }

    public EntityException(string? message, Exception? exception) : base(message, exception)
    {

    }

    [DoesNotReturn]
    public override void ThrowNoStackTrace(string message)
        => throw ExceptionDispatchInfo.Capture(new EntityException(message)).SourceException;

    [DoesNotReturn]
    public override void ThrowWithStackTrace(Exception exception)
        => ExceptionDispatchInfo.Capture(exception).Throw();
}
