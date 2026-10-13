using Bomberos.Core.Application.DTOs.Auth;
using Bomberos.Core.Application.DTOs.Usuarios;
using Bomberos.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bomberos.WebApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService auth, IUsuarioService usuarios) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(AuthRequest request)
    {
        var response = await auth.LoginAsync(request);
        return response.Success ? Ok(response) : Unauthorized(response);
    }

    /// <summary>Registro público de clientes. El usuario creado solo puede registrar emergencias.</summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<ActionResult<UsuarioDto>> Registro(RegistroClienteDto dto)
        => StatusCode(StatusCodes.Status201Created, await usuarios.RegistrarClienteAsync(dto));
}
