using Infrastructure.Data_Transfer_Object.Authorization;

namespace Infrastructure.Abstractions;

public interface IPublisher<TAuthorization> where TAuthorization : AuthorizationDetails
{
    public string Publish(TAuthorization details);
}
