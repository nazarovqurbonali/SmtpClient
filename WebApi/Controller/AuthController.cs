using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data.DTOs.AuthDto;
using WebApi.Data.Response;
using WebApi.Services.AuthService;

namespace WebApi.Controller;

[Route("[controller]")]
[ApiController]
public class AccountController(IAccountService accountService) : ControllerBase
{
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (ModelState.IsValid)
        {
            var response = await accountService.Register(registerDto);
            return StatusCode(response.StatusCode, response);
        }
        else
        {
            var errorMessages = ModelState.SelectMany(e => e.Value.Errors.Select(e => e.ErrorMessage)).ToList();
            var response = new Response<RegisterDto>(HttpStatusCode.BadRequest, errorMessages);
            return StatusCode(response.StatusCode, response);
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto registerDto)
    {
        if (ModelState.IsValid)
        {
            var response = await accountService.Login(registerDto);
            return StatusCode(response.StatusCode, response);
        }
        else
        {
            var errorMessages = ModelState.SelectMany(e => e.Value.Errors.Select(e => e.ErrorMessage)).ToList();
            var response = new Response<RegisterDto>(HttpStatusCode.BadRequest, errorMessages);
            return StatusCode(response.StatusCode, response);
        }
    }

    [HttpPost("AddUserToRole")]
    [Authorize]
    public async Task<Response<string>> AddUserToRole(UserRoleDto userRoleDto)
        => await accountService.AddOrRemoveUserFromRole(userRoleDto, false);

    [HttpPut("ChangePassword")]
    [Authorize]
    public async Task<Response<string>> ChangePassword(ChangePasswordDto passwordDto)
    {
        var id = User.Claims.FirstOrDefault(c => c.Type == "sid")?.Value;
        return await accountService.ChangePassword(passwordDto, id!);
    }

    [HttpDelete("DeleteRoleFromUser")]
    [Authorize]
    public async Task<Response<string>> DeleteRoleFromUser(UserRoleDto userRoleDto)
        => await accountService.AddOrRemoveUserFromRole(userRoleDto, true);


    [HttpDelete("ForgotPassword")]
    public async Task<Response<string>> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        => await accountService.ForgotPasswordTokenGenerator(forgotPasswordDto);


    [HttpDelete("ResetPassword")]
    public async Task<Response<string>> ResetPassword(ResetPasswordDto resetPasswordDto)
        => await accountService.ResetPassword(resetPasswordDto);
}