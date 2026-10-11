namespace SharedStructures.Dtos.Auth;

public class LoginResponseDto
{
    public required string Token { get; set; }
    public required DateTime Expiration{ get; set; }
}