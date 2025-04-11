namespace BuildingBlocks.Extensions.Http;

public static class HttpAccessor
{
    public static readonly Guid SystemId = new("11111111-1111-1111-1111-111111111111");

    public static Guid GetId(this IHttpContextAccessor accessor)
        => accessor.HttpContext?.User.Claims
            .FirstOrDefault(x => x.Type == CustomClaimTypes.Id)?
            .Value is { } userIdString && Guid.TryParse(userIdString, out Guid userId)
            ? userId
            : throw new ArgumentNullException(nameof(userId));

    public static string? GetUserAgent(this IHttpContextAccessor accessor)
        => accessor.HttpContext?.Request.Headers["User-Agent"].ToString();

    public static string GetRemoteIpAddress(this IHttpContextAccessor accessor)
        => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
}