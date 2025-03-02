namespace Infrastructure.Data_Transfer_Object.Authorization;

public class RefreshDetails : AuthorizationDetails
{
    public int CombineCount { get; set; } = 3;
}
