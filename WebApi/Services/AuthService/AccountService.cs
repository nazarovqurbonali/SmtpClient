using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Text;
using WebApi.Data;
using WebApi.Data.DTOs.AuthDto;
using WebApi.Data.DTOs.EmailDto;
using WebApi.Data.Response;
using WebApi.Services.EmailService;

namespace WebApi.Services.AuthService;

public class AccountService : IAccountService
{
    #region ctor

    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly DataContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;

    public AccountService(
        UserManager<IdentityUser> userManager,
        IConfiguration configuration,
        DataContext context,
        RoleManager<IdentityRole> roleManager, IEmailService emailService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _context = context;
        _roleManager = roleManager;
        _emailService = emailService;
    }

    #endregion

    #region Register

    public async Task<Response<RegisterDto>> Register(RegisterDto model)
    {
        var existing = await _userManager.FindByNameAsync(model.Username);
        if (existing != null) return new Response<RegisterDto>(HttpStatusCode.BadRequest, "Already exist");

        var mapped = new IdentityUser()
        {
            UserName = model.Username,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };


        var response = await _userManager.CreateAsync(mapped, model.Password);
        if (response.Succeeded)
            return new Response<RegisterDto>(model);
        return new Response<RegisterDto>(HttpStatusCode.BadRequest, "something is wrong");
    }

    #endregion

    #region AddOrRemoveUserFromRole

    public async Task<Response<string>> AddOrRemoveUserFromRole(UserRoleDto userRole, bool delete = false)
    {
        var role = await _roleManager.FindByIdAsync(userRole.RoleId);
        var user = await _userManager.FindByIdAsync(userRole.UserId);
        if (role == null || user == null)
            return new Response<string>(HttpStatusCode.BadRequest, "Role or User not found");
        if (delete)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
            return new Response<string>(HttpStatusCode.OK, "removed");
        }

        var userInRole = await _userManager.IsInRoleAsync(user, role.Name!);
        if (userInRole) return new Response<string>(HttpStatusCode.BadRequest, "Role exists");

        await _userManager.AddToRoleAsync(user, role.Name!);
        return new Response<string>(HttpStatusCode.OK, "done");
    }

    #endregion

    #region Login

    public async Task<Response<string>> Login(LoginDto login)
    {
        var user = await _userManager.FindByNameAsync(login.Username);
        if (user == null)
            return new Response<string>(HttpStatusCode.BadRequest, "login or password is incorrect");
        var checkPassword = await _userManager.CheckPasswordAsync(user, login.Password);
        if (checkPassword)
        {
            var token = await GenerateJwtToken(user);
            return new Response<string>(token);
        }

        return new Response<string>(HttpStatusCode.BadRequest, "login or password is incorrect");
    }

    #endregion

    #region GenerateJwtToken

    private async Task<string> GenerateJwtToken(IdentityUser user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
        var securityKey = new SymmetricSecurityKey(key);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Name, user.UserName!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Sid, user.Id),
        };

        //add roles
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );


        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenString;
    }

    #endregion

    #region ChangePassword

    public async Task<Response<string>> ChangePassword(ChangePasswordDto passwordDto, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        var checkPassword = await _userManager.CheckPasswordAsync(user!, passwordDto.OldPassword);
        if (checkPassword == false)
        {
            return new Response<string>(HttpStatusCode.BadRequest, "password is incorrect");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user!);
        var result = await _userManager.ResetPasswordAsync(user!, token, passwordDto.Password);
        if (result.Succeeded)
            return new Response<string>(HttpStatusCode.OK, "success");
        return new Response<string>(HttpStatusCode.BadRequest, "could not reset your password");
    }

    #endregion

    #region ForgotPasswordTokenGenerator

    public async Task<Response<string>> ForgotPasswordTokenGenerator(ForgotPasswordDto forgotPasswordDto)
    {
        var existing = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
        if (existing == null) return new Response<string>(HttpStatusCode.BadRequest, "not found");
        var token = await _userManager.GeneratePasswordResetTokenAsync(existing);
        var url = $"http://localhost:5271/account/resetpassword?token={token}&email={forgotPasswordDto.Email}";
        _emailService.SendEmail(
            new MessageDto(new[] { forgotPasswordDto.Email }, "reset password",
                $"<h1><a href=\"{url}\">reset password</a></h1>"), TextFormat.Html);

        return new Response<string>(HttpStatusCode.OK, "reset password has been sent");
    }

    #endregion

    #region ResetPassword

    public async Task<Response<string>> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
            return new Response<string>(HttpStatusCode.BadRequest, "user not found");

        var resetPassResult =
            await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
        if (resetPassResult.Succeeded)
            return new Response<string>(HttpStatusCode.OK, "success");
        return new Response<string>(HttpStatusCode.BadRequest, "please try again");
    }

    #endregion
}