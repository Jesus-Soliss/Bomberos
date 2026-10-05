using Bomberos.Core.Application.DTOs.Consultas;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Domain.Entities;
using Bomberos.Core.Domain.Enums;

namespace Bomberos.Core.Application.Interfaces.Repositories;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    IQueryable<T> Query();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

/// <summary>Agrupa varios guardados en una sola transacción de base de datos.</summary>
public interface IUnitOfWork
{
    Task EjecutarEnTransaccionAsync(Func<Task> operacion);
}

public interface IEstacionRepository : IGenericRepository<Estacion>
{
    Task<bool> ExisteNombreAsync(string nombre, int? exceptoId = null);
}

public interface IBomberoRepository : IGenericRepository<Bombero>
{
    Task<List<Bombero>> GetAllConEstacionAsync();
    Task<Bombero?> GetConEstacionAsync(int id);
    Task<bool> ExisteCedulaAsync(string cedula, int? exceptoId = null);
    /// <summary>Bomberos ACTIVOS sin asignación ACTIVA.</summary>
    Task<List<Bombero>> GetDisponiblesAsync();
    Task<int> ContarActivosAsync();
}

public interface IVehiculoRepository : IGenericRepository<Vehiculo>
{
    Task<List<Vehiculo>> GetAllConEstacionAsync();
    Task<Vehiculo?> GetConEstacionAsync(int id);
    Task<bool> ExistePlacaAsync(string placa, int? exceptoId = null);
    Task<List<Vehiculo>> GetDisponiblesAsync();
    Task<Dictionary<EstadoVehiculo, int>> ContarPorEstadoAsync();
}

public interface IEmergenciaRepository : IGenericRepository<Emergencia>
{
    Task<List<Emergencia>> BuscarAsync(EmergenciaCriterio criterio);
    /// <summary>Emergencia con sus asignaciones de bomberos y vehículos.</summary>
    Task<Emergencia?> GetDetalleAsync(int id);
    Task<Emergencia?> GetPorReferenciaAsync(string referenciaExterna);
    Task<bool> ExisteReferenciaAsync(string referenciaExterna);
    Task<bool> TieneAsignacionesActivasAsync(int emergenciaId);
    Task<int> ContarAsync(DateTime? desde = null);
    Task<Dictionary<EstadoEmergencia, int>> ContarPorEstadoAsync();
    Task<Dictionary<OrigenEmergencia, int>> ContarPorOrigenAsync();
    Task<List<Emergencia>> GetUltimasAsync(int cantidad);
}

public interface IAsignacionRepository
{
    Task<AsignacionBombero?> GetBomberoAsync(int emergenciaId, int bomberoId);
    Task<AsignacionVehiculo?> GetVehiculoAsync(int emergenciaId, int vehiculoId);
    Task<bool> BomberoTieneAsignacionActivaAsync(int bomberoId);
    Task<bool> VehiculoTieneAsignacionActivaAsync(int vehiculoId);
    Task AddBomberoAsync(AsignacionBombero asignacion);
    Task AddVehiculoAsync(AsignacionVehiculo asignacion);
    Task UpdateBomberoAsync(AsignacionBombero asignacion);
    Task UpdateVehiculoAsync(AsignacionVehiculo asignacion);
    /// <summary>Ids de los bomberos con una asignación ACTIVA.</summary>
    Task<HashSet<int>> GetBomberosEnServicioAsync();
    Task<List<AsignacionBombero>> ListarBomberosAsync(EstadoAsignacion? estado);
    Task<List<AsignacionVehiculo>> ListarVehiculosAsync(EstadoAsignacion? estado);
}

public interface ILogRepository
{
    Task<List<AuditLog>> BuscarAuditoriaAsync(LogFiltroDto filtro);
    Task<List<RequestLog>> BuscarPeticionesAsync(LogFiltroDto filtro);
}
