namespace API.Controllers.Base;

public static class ApiVersions
{
    public const string V1 = "v1";
    public const string V2 = "v2";
}

public static class ApiAddress
{
    public const string Base = $"api/{ApiVersions.V1}";
}