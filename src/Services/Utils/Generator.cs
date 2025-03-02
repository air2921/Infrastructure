using Infrastructure.Abstractions;
using Infrastructure.Exceptions;
using System.Text;

namespace Infrastructure.Services.Utils;

public class Generator : IGenerator
{
    private static readonly Random _rnd = new();

    public string GuidCombine(int count, bool useNoHyphensFormat = false)
    {
        if (count < Immutable.MinGuidCombineLength || count >= Immutable.MaxGuidCombineLength)
            throw new InvalidAgrumentException($"Combine of {count} guid is not supported");

        var builder = new StringBuilder(useNoHyphensFormat ? 32 : 36 * count);
        for (int i = 0; i < count; i++)
            builder.Append(Guid.NewGuid().ToString(useNoHyphensFormat ? "N" : string.Empty));

        return builder.ToString();
    }

    public int GenerateCode(int length)
    {
        if (length < Immutable.MinCodeLength || length >= Immutable.MaxCodeLength)
            throw new InvalidAgrumentException($"Lenght {length} is not supported");

        var builder = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            builder.Append(_rnd.Next(10));

        return int.Parse(builder.ToString());
    }
}
