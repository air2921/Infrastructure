namespace Infrastructure.Abstractions;

public interface IGenerator
{
    public string GuidCombine(int count, bool useNoHyphensFormat = false);
    public int GenerateCode(int length);
}
