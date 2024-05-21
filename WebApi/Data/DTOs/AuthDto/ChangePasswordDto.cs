using System.ComponentModel.DataAnnotations;

namespace WebApi.Data.DTOs.AuthDto;

public class ChangePasswordDto
{
    public required string OldPassword { get; set; }
    [DataType(DataType.Password)] public required string Password { get; set; }

    [Compare("Password")] public required string ConfirmPassword { get; set; }
}