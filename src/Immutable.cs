namespace Infrastructure;

public static class Immutable
{
    public const string AspNetCoreEnv = "ASPNETCORE_ENVIRONMENT";
    public const string EnvProperty = "Environment";

    public const int GetRangeAwait = 20;
    public const int GetByFilterAwait = 20;
    public const int GetByIdAwait = 20;
    public const int AddAwait = 20;
    public const int AddRangeAwait = 20;
    public const int RemoveByIdAwait = 20;
    public const int RemoveRangeAwait = 90;
    public const int RemoveByFilterAwait = 20;
    public const int UpdateAwait = 20;
    public const int UpdateRangeAwait = 60;
    public const int RestoreAwait = 20;
    public const int RestoreRangeAwait = 60;

    public const int MinCodeLength = 1;
    public const int MaxCodeLength = 10;

    public const int MinGuidCombineLength = 1;
    public const int MaxGuidCombineLength = 10;

    public const string ReadebleChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    public const string HexChars = "0123456789ABCDEF";
}
