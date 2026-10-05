using Bomberos.Core.Domain.Common;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Domain.Entities;

public class Estacion : AuditableBaseEntity
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string? Telefono { get; set; }
    public EstadoEstacion Estado { get; set; } = EstadoEstacion.Activa;

    public ICollection<Bombero> Bomberos { get; set; } = new List<Bombero>();
    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
