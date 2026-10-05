using System.ComponentModel.DataAnnotations;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.ViewModels;

public class EstacionViewModel
{
    public int Id { get; set; }

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(100, ErrorMessage = Validacion.Longitud)]
    public string Nombre { get; set; } = "";

    [Display(Name = "Dirección")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(200, ErrorMessage = Validacion.Longitud)]
    public string Direccion { get; set; } = "";

    [Display(Name = "Teléfono")]
    [StringLength(15, ErrorMessage = Validacion.Longitud)]
    [RegularExpression(@"^[0-9+\-() ]*$", ErrorMessage = "El teléfono solo admite números, espacios y los símbolos + - ( ).")]
    public string? Telefono { get; set; }
}

public class BomberoViewModel
{
    public int Id { get; set; }

    [Display(Name = "Nombre")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Nombre { get; set; } = "";

    [Display(Name = "Apellido")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(50, ErrorMessage = Validacion.Longitud)]
    public string Apellido { get; set; } = "";

    [Display(Name = "Cédula")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(15, ErrorMessage = Validacion.Longitud)]
    [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "La cédula solo admite números y guiones.")]
    public string Cedula { get; set; } = "";

    [Display(Name = "Rango")]
    [Required(ErrorMessage = Validacion.Requerido)]
    public RangoBombero? Rango { get; set; }

    [Display(Name = "Estación")]
    [Required(ErrorMessage = Validacion.Requerido)]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una estación.")]
    public int? EstacionId { get; set; }
}

public class VehiculoViewModel
{
    public int Id { get; set; }

    [Display(Name = "Placa")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(10, ErrorMessage = Validacion.Longitud)]
    public string Placa { get; set; } = "";

    [Display(Name = "Tipo")]
    [Required(ErrorMessage = Validacion.Requerido)]
    public TipoVehiculo? Tipo { get; set; }

    [Display(Name = "Estación")]
    [Required(ErrorMessage = Validacion.Requerido)]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una estación.")]
    public int? EstacionId { get; set; }
}

public class EmergenciaViewModel
{
    public int Id { get; set; }

    [Display(Name = "Tipo")]
    [Required(ErrorMessage = Validacion.Requerido)]
    public TipoEmergencia? Tipo { get; set; }

    [Display(Name = "Descripción")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(500, ErrorMessage = Validacion.Longitud)]
    public string Descripcion { get; set; } = "";

    [Display(Name = "Dirección")]
    [Required(ErrorMessage = Validacion.Requerido), StringLength(200, ErrorMessage = Validacion.Longitud)]
    public string Direccion { get; set; } = "";

    [Display(Name = "Prioridad")]
    [Required(ErrorMessage = Validacion.Requerido)]
    public PrioridadEmergencia? Prioridad { get; set; }
}

public class EmergenciaFiltroViewModel
{
    public EstadoEmergencia? Estado { get; set; }
    public TipoEmergencia? Tipo { get; set; }
    public PrioridadEmergencia? Prioridad { get; set; }
    public OrigenEmergencia? Origen { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }
}

public class EmergenciasIndexViewModel
{
    public EmergenciaFiltroViewModel Filtro { get; set; } = new();
    public List<EmergenciaDto> Emergencias { get; set; } = [];
    /// <summary>CORE no responde: no hay listado, pero se puede registrar y asignar (queda pendiente).</summary>
    public bool Contingencia { get; set; }
}

/// <summary>Bombero o vehículo disponible, tal como lo entrega Integración (desde CORE o desde su copia local).</summary>
public class RecursoDisponibleViewModel
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = "";
    public string Estado { get; set; } = "";
}

public class EmergenciaDetalleViewModel
{
    public int Id { get; set; }
    /// <summary>Nulo en modo contingencia (CORE caído).</summary>
    public EmergenciaDetalleDto? Emergencia { get; set; }
    public List<RecursoDisponibleViewModel> BomberosDisponibles { get; set; } = [];
    public List<RecursoDisponibleViewModel> VehiculosDisponibles { get; set; } = [];
    public bool Contingencia => Emergencia is null;
    /// <summary>La lista de disponibles vino de la copia local de Integración.</summary>
    public bool DisponiblesDeCopiaLocal { get; set; }
}

public class LogFiltroViewModel
{
    public string? UsuarioId { get; set; }
    public string? Accion { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }
}

/// <summary>Operación que Integración recibió con CORE caído.</summary>
public class OperacionPendienteViewModel
{
    public long Id { get; set; }
    public DateTime FechaHora { get; set; }
    public string? UsuarioId { get; set; }
    public string Tipo { get; set; } = "";
    public string Metodo { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string? ReferenciaProvisional { get; set; }
    public string Estado { get; set; } = "";
    public string? Motivo { get; set; }
    public int Reintentos { get; set; }
    public DateTime? FechaAplicacion { get; set; }
}
