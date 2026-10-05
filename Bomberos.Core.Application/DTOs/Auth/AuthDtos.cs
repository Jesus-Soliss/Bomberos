using System.ComponentModel.DataAnnotations;

namespace Bomberos.Core.Application.DTOs.Auth;

public class AuthRequest
{
    [Required(ErrorMessage = "El usuario es requerido.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "La clave es requerida.")]
    public string Password { get; set; } = "";
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public DateTime? Expira { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? NombreCompleto { get; set; }
    public IList<string> Roles { get; set; } = [];

    public static AuthResponse Fail(string message) => new() { Success = false, Message = message };
}
