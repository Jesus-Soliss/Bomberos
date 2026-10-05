using Bomberos.Core.Application.DTOs.Auth;
using Bomberos.Core.Application.DTOs.Consultas;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.DTOs.Recursos;
using Bomberos.Core.Application.DTOs.Usuarios;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.Interfaces.Services;

/// <summary>Usuario autenticado en la petición actual (lo implementa la WebApi).</summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Username { get; }
}

public interface IAuditService
{
    /// <param name="usuarioId">Si es nulo se usa el usuario autenticado.</param>
    Task RegistrarAsync(string accion, string entidad, int? entidadId, ResultadoAuditoria resultado, string? usuarioId = null);
}

// ---------- Implementados en Infrastructure.Identity ----------

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(AuthRequest request);
}

public interface IUsuarioService
{
    Task<List<UsuarioDto>> GetAllAsync();
    Task<UsuarioDto> GetByIdAsync(string id);
    Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto);
    Task<UsuarioDto> EditarAsync(string id, EditarUsuarioDto dto);
    Task<UsuarioDto> CambiarEstadoAsync(string id, string estado);
    Task<UsuarioDto> CambiarRolAsync(string id, string rol);
    Task RestablecerPasswordAsync(string id, string nuevaPassword);
}

public interface IRolService
{
    Task<List<RolDto>> GetAllAsync();
    Task<RolDto> CrearAsync(GuardarRolDto dto);
    Task<RolDto> EditarAsync(string id, GuardarRolDto dto);
    Task EliminarAsync(string id);
}

// ---------- Implementados en Application ----------

public interface IEstacionService
{
    Task<List<EstacionDto>> GetAllAsync();
    Task<EstacionDto> GetByIdAsync(int id);
    Task<EstacionDto> CrearAsync(GuardarEstacionDto dto);
    Task<EstacionDto> EditarAsync(int id, GuardarEstacionDto dto);
    Task<EstacionDto> CambiarEstadoAsync(int id, EstadoEstacion estado);
}

public interface IBomberoService
{
    Task<List<BomberoDto>> GetAllAsync();
    Task<List<BomberoDto>> GetDisponiblesAsync();
    Task<BomberoDto> GetByIdAsync(int id);
    Task<BomberoDto> CrearAsync(GuardarBomberoDto dto);
    Task<BomberoDto> EditarAsync(int id, GuardarBomberoDto dto);
    Task<BomberoDto> CambiarEstadoAsync(int id, EstadoBombero estado);
}

public interface IVehiculoService
{
    Task<List<VehiculoDto>> GetAllAsync();
    Task<List<VehiculoDto>> GetDisponiblesAsync();
    Task<VehiculoDto> GetByIdAsync(int id);
    Task<VehiculoDto> CrearAsync(GuardarVehiculoDto dto);
    Task<VehiculoDto> EditarAsync(int id, GuardarVehiculoDto dto);
    Task<VehiculoDto> CambiarEstadoAsync(int id, EstadoVehiculo estado);
}

public interface IEmergenciaService
{
    Task<List<EmergenciaDto>> BuscarAsync(EmergenciaFiltroDto filtro);
    Task<EmergenciaDetalleDto> GetDetalleAsync(int id);
    Task<EmergenciaDto> GetPorReferenciaAsync(string referenciaExterna);
    Task<EmergenciaDto> CrearAsync(GuardarEmergenciaDto dto);
    Task<EmergenciaDto> CrearExternaAsync(ReporteExternoDto dto);
    Task<EmergenciaDto> EditarAsync(int id, GuardarEmergenciaDto dto);
    Task<EmergenciaDto> CambiarEstadoAsync(int id, EstadoEmergencia estado);
}

public interface IAsignacionService
{
    Task<AsignacionBomberoDto> AsignarBomberoAsync(int emergenciaId, int bomberoId);
    Task<AsignacionVehiculoDto> AsignarVehiculoAsync(int emergenciaId, int vehiculoId);
    /// <param name="nuevoEstado">FINALIZADA o CANCELADA.</param>
    Task<AsignacionBomberoDto> CerrarBomberoAsync(int emergenciaId, int bomberoId, EstadoAsignacion nuevoEstado);
    Task<AsignacionVehiculoDto> CerrarVehiculoAsync(int emergenciaId, int vehiculoId, EstadoAsignacion nuevoEstado);
    Task<AsignacionesDto> ListarAsync(string? estado);
}

public interface IConsultaService
{
    Task<DashboardDto> GetDashboardAsync();
    Task<List<AuditLogDto>> GetAuditoriaAsync(LogFiltroDto filtro);
    Task<List<RequestLogDto>> GetRequestLogsAsync(LogFiltroDto filtro);
}
