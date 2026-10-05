using Bomberos.Core.Application.Common;
using Microsoft.AspNetCore.Identity;

namespace Bomberos.Infrastructure.Identity.Entities;

public class ApplicationUser : IdentityUser
{
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Estado { get; set; } = EstadosUsuario.Activo; // ACTIVO o INACTIVO
}
