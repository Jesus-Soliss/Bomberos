using System.ComponentModel.DataAnnotations;

namespace Bomberos.Core.Application.DTOs.Usuarios;

public class UsuarioDto
{
    public string Id { get; set; } = "";
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Estado { get; set; } = "";
    public string? Rol { get; set; }
}

public class CrearUsuarioDto
{
    [Required, StringLength(50)]
    public string Username { get; set; } = "";

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = "";

    [Required, StringLength(50)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(50)]
    public string Apellido { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";

    [Required]
    public string Rol { get; set; } = "";
}

public class EditarUsuarioDto
{
    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = "";

    [Required, StringLength(50)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(50)]
    public string Apellido { get; set; } = "";
}

public class CambiarEstadoUsuarioDto
{
    /// <summary>ACTIVO o INACTIVO.</summary>
    [Required]
    public string Estado { get; set; } = "";
}

public class CambiarRolDto
{
    [Required]
    public string Rol { get; set; } = "";
}

public class RestablecerPasswordDto
{
    [Required]
    public string NuevaPassword { get; set; } = "";
}

public class RolDto
{
    public string Id { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int Usuarios { get; set; }
}

public class GuardarRolDto
{
    [Required, StringLength(50)]
    public string Nombre { get; set; } = "";
}
