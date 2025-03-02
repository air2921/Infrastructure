namespace Infrastructure.Configuration;

public abstract class Validator
{
    public abstract bool IsValidConfigure();

    public bool IsEnable { get; set; }
}