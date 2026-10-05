namespace Bomberos.Integration.Api.Data;

/// <summary>Cada petición recibida por Integración y su respuesta.</summary>
public class IntegrationLog
{
    public long Id { get; set; }
    public DateTime FechaHora { get; set; }
    /// <summary>Usuario del JWT, WEBAPP (anónimo) o CENTRAL_911.</summary>
    public string Origen { get; set; } = "";
    public string Metodo { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string? Peticion { get; set; }
    public int CodigoRespuesta { get; set; }
    public string? Respuesta { get; set; }
    /// <summary>La respuesta se generó con la copia local porque CORE no respondió.</summary>
    public bool AtendidoLocal { get; set; }
}

/// <summary>Operación recibida mientras CORE estaba caído; se reenvía cuando vuelve.</summary>
public class OperacionPendiente
{
    public long Id { get; set; }
    public DateTime FechaHora { get; set; }
    public string? UsuarioId { get; set; }
    public string Tipo { get; set; } = "";
    public string Metodo { get; set; } = "";
    public string Endpoint { get; set; } = "";
    public string? Cuerpo { get; set; }
    public string? ReferenciaProvisional { get; set; }
    public string Estado { get; set; } = EstadosPendiente.Pendiente;
    public string? Motivo { get; set; }
    public int Reintentos { get; set; }
    public DateTime? FechaAplicacion { get; set; }
}

/// <summary>Copia local de bomberos y vehículos para responder si CORE cae.</summary>
public class CacheRecurso
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "";
    /// <summary>Id del recurso en CORE.</summary>
    public int ExternalId { get; set; }
    public string Descripcion { get; set; } = "";
    public string Estado { get; set; } = "";
    public DateTime UltimaActualizacion { get; set; }
}

public static class EstadosPendiente
{
    public const string Pendiente = "PENDIENTE";
    public const string Aplicada = "APLICADA";
    public const string Rechazada = "RECHAZADA";
}

public static class TiposOperacion
{
    public const string CrearEmergencia = "CREAR_EMERGENCIA";
    public const string AsignarBombero = "ASIGNAR_BOMBERO";
    public const string AsignarVehiculo = "ASIGNAR_VEHICULO";
    public const string FinalizarAsignacion = "FINALIZAR_ASIGNACION";
    public const string Reporte911 = "REPORTE_911";
}

public static class TiposRecurso
{
    public const string Bombero = "BOMBERO";
    public const string Vehiculo = "VEHICULO";
}

public static class EstadosRecurso
{
    public const string Disponible = "DISPONIBLE";
    public const string EnServicio = "EN_SERVICIO";
    /// <summary>Asignado por Integración mientras CORE está caído.</summary>
    public const string Reservado = "RESERVADO";
    public const string NoDisponible = "NO_DISPONIBLE";
}
