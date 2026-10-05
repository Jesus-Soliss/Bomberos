using System.ComponentModel.DataAnnotations;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.DTOs.Recursos;

// ---------- Estaciones ----------

public class EstacionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string? Telefono { get; set; }
    public EstadoEstacion Estado { get; set; }
}

public class GuardarEstacionDto
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(200)]
    public string Direccion { get; set; } = "";

    [StringLength(15)]
    public string? Telefono { get; set; }
}

public class CambiarEstadoEstacionDto
{
    [Required(ErrorMessage = "El campo {0} es requerido.")]
    [EnumDataType(typeof(EstadoEstacion), ErrorMessage = "El valor del campo {0} no es válido.")]
    public EstadoEstacion? Estado { get; set; }
}

// ---------- Bomberos ----------

public class BomberoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Cedula { get; set; } = "";
    public RangoBombero Rango { get; set; }
    public int EstacionId { get; set; }
    public string EstacionNombre { get; set; } = "";
    public EstadoBombero Estado { get; set; }
    /// <summary>Tiene una asignación ACTIVA en este momento.</summary>
    public bool EnServicio { get; set; }
}

public class GuardarBomberoDto
{
    [Required, StringLength(50)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(50)]
    public string Apellido { get; set; } = "";

    [Required, StringLength(15)]
    public string Cedula { get; set; } = "";

    [Required(ErrorMessage = "El campo {0} es requerido.")]
    [EnumDataType(typeof(RangoBombero), ErrorMessage = "El valor del campo {0} no es válido.")]
    public RangoBombero? Rango { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La estación es requerida.")]
    public int EstacionId { get; set; }
}

public class CambiarEstadoBomberoDto
{
    [Required(ErrorMessage = "El campo {0} es requerido.")]
    [EnumDataType(typeof(EstadoBombero), ErrorMessage = "El valor del campo {0} no es válido.")]
    public EstadoBombero? Estado { get; set; }
}

// ---------- Vehículos ----------

public class VehiculoDto
{
    public int Id { get; set; }
    public string Placa { get; set; } = "";
    public TipoVehiculo Tipo { get; set; }
    public int EstacionId { get; set; }
    public string EstacionNombre { get; set; } = "";
    public EstadoVehiculo Estado { get; set; }
}

public class GuardarVehiculoDto
{
    [Required, StringLength(10)]
    public string Placa { get; set; } = "";

    [Required(ErrorMessage = "El campo {0} es requerido.")]
    [EnumDataType(typeof(TipoVehiculo), ErrorMessage = "El valor del campo {0} no es válido.")]
    public TipoVehiculo? Tipo { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La estación es requerida.")]
    public int EstacionId { get; set; }
}

public class CambiarEstadoVehiculoDto
{
    [Required(ErrorMessage = "El campo {0} es requerido.")]
    [EnumDataType(typeof(EstadoVehiculo), ErrorMessage = "El valor del campo {0} no es válido.")]
    public EstadoVehiculo? Estado { get; set; }
}
