using System.ComponentModel.DataAnnotations;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.DTOs.Emergencias;

public class EmergenciaDto
{
    public int Id { get; set; }
    public TipoEmergencia Tipo { get; set; }
    public string Descripcion { get; set; } = "";
    public string Direccion { get; set; } = "";
    public DateTime FechaHora { get; set; }
    public PrioridadEmergencia Prioridad { get; set; }
    public EstadoEmergencia Estado { get; set; }
    public OrigenEmergencia Origen { get; set; }
    public string? ReferenciaExterna { get; set; }
}

public class EmergenciaDetalleDto : EmergenciaDto
{
    public List<AsignacionBomberoDto> Bomberos { get; set; } = [];
    public List<AsignacionVehiculoDto> Vehiculos { get; set; } = [];
}

public class GuardarEmergenciaDto
{
    [Required]
    public TipoEmergencia? Tipo { get; set; }

    [Required, StringLength(500)]
    public string Descripcion { get; set; } = "";

    [Required, StringLength(200)]
    public string Direccion { get; set; } = "";

    [Required]
    public PrioridadEmergencia? Prioridad { get; set; }
}

/// <summary>Reporte recibido del Centro 911 (llega a través de Integración).</summary>
public class ReporteExternoDto : GuardarEmergenciaDto
{
    [Required, StringLength(50)]
    public string Referencia { get; set; } = "";
}

public class CambiarEstadoEmergenciaDto
{
    [Required]
    public EstadoEmergencia? Estado { get; set; }
}

/// <summary>Filtros del listado, tal como llegan en la query string (EN_ATENCION, CENTRAL_911, ...).</summary>
public class EmergenciaFiltroDto
{
    public string? Estado { get; set; }
    public string? Tipo { get; set; }
    public string? Prioridad { get; set; }
    public string? Origen { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

/// <summary>Filtros ya validados, para el repositorio.</summary>
public class EmergenciaCriterio
{
    public EstadoEmergencia? Estado { get; set; }
    public TipoEmergencia? Tipo { get; set; }
    public PrioridadEmergencia? Prioridad { get; set; }
    public OrigenEmergencia? Origen { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

// ---------- Asignaciones ----------

public class AsignarBomberoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El bombero es requerido.")]
    public int BomberoId { get; set; }
}

public class AsignarVehiculoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El vehículo es requerido.")]
    public int VehiculoId { get; set; }
}

public class AsignacionBomberoDto
{
    public int EmergenciaId { get; set; }
    public int BomberoId { get; set; }
    public string BomberoNombre { get; set; } = "";
    public string Cedula { get; set; } = "";
    public RangoBombero Rango { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public EstadoAsignacion Estado { get; set; }
}

public class AsignacionVehiculoDto
{
    public int EmergenciaId { get; set; }
    public int VehiculoId { get; set; }
    public string Placa { get; set; } = "";
    public TipoVehiculo TipoVehiculo { get; set; }
    public DateTime FechaAsignacion { get; set; }
    public DateTime? FechaFinalizacion { get; set; }
    public EstadoAsignacion Estado { get; set; }
}

public class AsignacionesDto
{
    public List<AsignacionBomberoDto> Bomberos { get; set; } = [];
    public List<AsignacionVehiculoDto> Vehiculos { get; set; } = [];
}
