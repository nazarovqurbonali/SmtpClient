using System.ComponentModel.DataAnnotations;

namespace WebApi.Data.DTOs.AuthDto;

public class RegisterDto
{
    public required string Username { get; set; }
    [DataType(DataType.Password)] public required string Password { get; set; }

    [Compare("Password")] public required string ConfirmPassword { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
}