using System.ComponentModel.DataAnnotations;

namespace Bomberos.Core.Application.ViewModels;

public static class Validacion
{
    public const string Requerido = "El campo {0} es requerido.";
    public const string Longitud = "El campo {0} admite un máximo de {1} caracteres.";
    // La misma política que aplica Identity en CORE (RN-22).
    public const string PatronPassword = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$";
    public const string MensajePassword =
        "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un símbolo.";
}

public class LoginViewModel
{
    [Display(Name = "Usuario")]
    [Required(ErrorMessage = Validacion.Requerido)]
    public string Username { get; set; } = "";

    [Display(Name = "Clave")]
    [Required(ErrorMessage = Validacion.Requerido)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class RegistroViewModel
{
    [Display(Name = "Usuario")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Username { get; set; } = "";

    [Display(Name = "Correo")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(100, ErrorMessage = Validacion.Longitud)]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = "";

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Nombre { get; set; } = "";

    [Display(Name = "Apellido")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Apellido { get; set; } = "";

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = Validacion.Requerido), DataType(DataType.Password)]
    [RegularExpression(Validacion.PatronPassword, ErrorMessage = Validacion.MensajePassword)]
    public string Password { get; set; } = "";

    [Display(Name = "Confirmar contraseña")]
    [Required(ErrorMessage = Validacion.Requerido), DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = "";
}

public class UsuarioCrearViewModel
{
    [Display(Name = "Usuario")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Username { get; set; } = "";

    [Display(Name = "Correo")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(100, ErrorMessage = Validacion.Longitud)]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = "";

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Nombre { get; set; } = "";

    [Display(Name = "Apellido")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Apellido { get; set; } = "";

    [Display(Name = "Perfil")]
    [Required(ErrorMessage = Validacion.Requerido)]
    public string Rol { get; set; } = "";

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = Validacion.Requerido), DataType(DataType.Password)]
    [RegularExpression(Validacion.PatronPassword, ErrorMessage = Validacion.MensajePassword)]
    public string Password { get; set; } = "";

    [Display(Name = "Confirmar contraseña")]
    [Required(ErrorMessage = Validacion.Requerido), DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = "";
}

public class UsuarioEditarViewModel
{
    public string Id { get; set; } = "";

    [Display(Name = "Usuario")]
    public string Username { get; set; } = "";

    [Display(Name = "Correo")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(100, ErrorMessage = Validacion.Longitud)]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = "";

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Nombre { get; set; } = "";

    [Display(Name = "Apellido")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Apellido { get; set; } = "";
}

public class RestablecerPasswordViewModel
{
    public string Id { get; set; } = "";

    [Display(Name = "Usuario")]
    public string Username { get; set; } = "";

    [Display(Name = "Nueva contraseña")]
    [Required(ErrorMessage = Validacion.Requerido), DataType(DataType.Password)]
    [RegularExpression(Validacion.PatronPassword, ErrorMessage = Validacion.MensajePassword)]
    public string NuevaPassword { get; set; } = "";

    [Display(Name = "Confirmar contraseña")]
    [Required(ErrorMessage = Validacion.Requerido), DataType(DataType.Password)]
    [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = "";
}

public class RolViewModel
{
    public string? Id { get; set; }

    [Display(Name = "Nombre del perfil")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Nombre { get; set; } = "";
}
