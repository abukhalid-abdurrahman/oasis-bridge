namespace Domain.Entities;

public class UserToken
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string Token { get; set; } = string.Empty;
    public TokenType TokenType { get; set; } = TokenType.AccessToken;
    
    public DateTimeOffset Expiration { get; set; }  
    public bool IsRevoked { get; set; }             
    public bool IsUsed { get; set; }               

    public string? IpAddress { get; set; }        
    public string? UserAgent { get; set; }  
}