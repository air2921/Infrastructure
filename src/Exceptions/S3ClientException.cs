using Infrastructure.Exceptions.Global;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Exceptions;

public class S3ClientException : InfrastructureException
{
    public S3ClientException() : base()
    {

    }

    public S3ClientException(string? message) : base(message)
    {

    }

    public S3ClientException(string? message, Exception? exception) : base(message, exception)
    {

    }

    public static void ThrowIf([DoesNotReturnIf(true)] bool condition, string message)
    {
        if (!condition)
            return;

        throw new S3ClientException(message);
    }

    public static void ThrowIfNull([NotNull] object? param, string message)
    {
        if (param is null)
            throw new S3ClientException(message);
    }
}
