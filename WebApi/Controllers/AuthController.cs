using Bomberos.Core.Application.DTOs.Auth;
using Bomberos.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.WebApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        var response = await auth.LoginAsync(request);
        return response.Success ? Ok(response) : Unauthorized(response);
    }
}
