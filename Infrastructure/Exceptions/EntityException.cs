using Infrastructure.Exceptions.Global;

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
}
