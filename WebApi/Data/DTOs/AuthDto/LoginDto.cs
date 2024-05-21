namespace WebApi.Data.DTOs.AuthDto;

public class LoginDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? ReturnUrl { get; set; }
}