using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Extensions;

public static class Jwt
{
    public static async Task<Result<LoginResponse>> GenerateTokenAsync(
        this DataContext dbContext,
        User user,
        IConfiguration config)
    {
        string key = config["Jwt:key"]!;

        SigningCredentials credentials =
            new SigningCredentials(GetSymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(CustomClaimTypes.Id, user.Id.ToString()),
            new(CustomClaimTypes.UserName, user.UserName),
            new(CustomClaimTypes.Email, user.Email),
            new(CustomClaimTypes.Phone, user.PhoneNumber),
            new(CustomClaimTypes.FirstName, user.FirstName ?? ""),
            new(CustomClaimTypes.LastName, user.LastName ?? ""),
            new(CustomClaimTypes.Code, user.Code.ToString()),
        ];

        claims.AddRange(await (from u in dbContext.Users
            join ur in dbContext.UserRoles on u.Id equals ur.UserId
            join r in dbContext.Roles on ur.RoleId equals r.Id
            where u.Id == user.Id
            select new Claim(CustomClaimTypes.Role, r.Name)).ToListAsync());


        DateTime current = DateTime.UtcNow;

        JwtSecurityToken jwt = new JwtSecurityToken(
            issuer: config["Jwt:issuer"],
            audience: config["Jwt:audience"],
            claims: claims,
            expires: current.AddMinutes(30),
            signingCredentials: credentials
        );

        return Result<LoginResponse>.Success(new(
            new JwtSecurityTokenHandler().WriteToken(jwt),
            current,
            current.AddMinutes(30)));
    }

    public static SymmetricSecurityKey GetSymmetricSecurityKey(string key) =>
        new(Encoding.UTF8.GetBytes(key));
}