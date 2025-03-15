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
}
