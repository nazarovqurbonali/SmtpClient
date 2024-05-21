using WebApi.Data.DTOs.AuthDto;
using WebApi.Data.Response;

namespace WebApi.Services.AuthService;

public interface IAccountService
{
    Task<Response<RegisterDto>> Register(RegisterDto model);
    Task<Response<string>> Login(LoginDto login);
    Task<Response<string>> AddOrRemoveUserFromRole(UserRoleDto userRole, bool delete = false);
    Task<Response<string>> ResetPassword(ResetPasswordDto resetPasswordDto);
    Task<Response<string>> ForgotPasswordTokenGenerator(ForgotPasswordDto forgotPasswordDto);
    Task<Response<string>> ChangePassword(ChangePasswordDto passwordDto, string userId);
}