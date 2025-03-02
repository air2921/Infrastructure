namespace Infrastructure.Exceptions.Global;

public class InfrastructureConfigurationException : ArgumentException
{
    public InfrastructureConfigurationException() : base()
    {

    }

    public InfrastructureConfigurationException(string? message) : base(message)
    {

    }

    public InfrastructureConfigurationException(string? message, string? argument) : base(message, argument)
    {

    }
}
