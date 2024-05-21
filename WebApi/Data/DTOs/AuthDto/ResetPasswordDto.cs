using System.ComponentModel.DataAnnotations;

namespace WebApi.Data.DTOs.AuthDto;

public class ResetPasswordDto
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    [DataType(DataType.Password)] public required string Password { get; set; }

    [Compare("Password")] public required string ConfirmPassword { get; set; }
}