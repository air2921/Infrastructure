using Infrastructure.Abstractions;
using Infrastructure.Data_Transfer_Object.Authorization;

namespace Infrastructure.Services.Authorization;

public class RefreshPublisher(IGenerator generator) : IPublisher<RefreshDetails>
{
    public string Publish(RefreshDetails details)
        => generator.GuidCombine(details.CombineCount);
}
