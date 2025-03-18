namespace Infrastructure.Extensions;

public static class Jwt
{
    public static async Task<Result<LoginResponse>> GenerateTokenAsync(
        this DataContext dbContext,
        User user,
        IConfiguration config)
    {
        string? key = config["Jwt:key"];
        string? issuer = config["Jwt:issuer"];
        string? audience = config["Jwt:audience"];

        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            throw new InvalidOperationException("JWT key, issuer, or audience is missing in configuration.");

        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(CustomClaimTypes.Id, user.Id.ToString()),
            new(CustomClaimTypes.UserName, user.UserName),
            new(CustomClaimTypes.Email, user.Email),
            new(CustomClaimTypes.Phone, user.PhoneNumber),
            new(CustomClaimTypes.FirstName, user.FirstName ?? ""),
            new(CustomClaimTypes.LastName, user.LastName ?? ""),
            new(CustomClaimTypes.TokenVersion, user.TokenVersion.ToString()),
        ];

        claims.AddRange(await dbContext.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => new Claim(CustomClaimTypes.Role, ur.Role.Name))
            .AsNoTracking() 
            .ToListAsync());

        DateTime current = DateTime.UtcNow;

        JwtSecurityToken jwt = new(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: current.AddMinutes(30),
            signingCredentials: credentials);

        return Result<LoginResponse>.Success(new(
            new JwtSecurityTokenHandler().WriteToken(jwt),
            current,
            current.AddMinutes(30)));
    }
}