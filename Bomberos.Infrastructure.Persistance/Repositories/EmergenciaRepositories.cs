using Bomberos.Core.Application.DTOs.Consultas;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Core.Domain.Entities;
using Bomberos.Core.Domain.Enums;
using Bomberos.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.Infrastructure.Persistence.Repositories;

public class EmergenciaRepository(ApplicationContext context) : GenericRepository<Emergencia>(context), IEmergenciaRepository
{
    public Task<List<Emergencia>> BuscarAsync(EmergenciaCriterio criterio)
    {
        var query = Context.Emergencias.AsNoTracking();
        if (criterio.Estado is { } estado) query = query.Where(e => e.Estado == estado);
        if (criterio.Tipo is { } tipo) query = query.Where(e => e.Tipo == tipo);
        if (criterio.Prioridad is { } prioridad) query = query.Where(e => e.Prioridad == prioridad);
        if (criterio.Origen is { } origen) query = query.Where(e => e.Origen == origen);
        if (criterio.Desde is { } desde) query = query.Where(e => e.FechaHora >= desde);
        if (Fechas.FinInclusivo(criterio.Hasta) is { } hasta) query = query.Where(e => e.FechaHora < hasta);
        return query.OrderByDescending(e => e.FechaHora).ToListAsync();
    }

    public Task<Emergencia?> GetDetalleAsync(int id)
        => Context.Emergencias.AsNoTracking().AsSplitQuery()
            .Include(e => e.AsignacionesBomberos).ThenInclude(a => a.Bombero)
            .Include(e => e.AsignacionesVehiculos).ThenInclude(a => a.Vehiculo)
            .FirstOrDefaultAsync(e => e.Id == id);

    public Task<Emergencia?> GetPorReferenciaAsync(string referenciaExterna)
        => Context.Emergencias.AsNoTracking().FirstOrDefaultAsync(e => e.ReferenciaExterna == referenciaExterna);

    public Task<bool> ExisteReferenciaAsync(string referenciaExterna)
        => Context.Emergencias.AnyAsync(e => e.ReferenciaExterna == referenciaExterna);

    public async Task<bool> TieneAsignacionesActivasAsync(int emergenciaId)
        => await Context.AsignacionesBomberos.AnyAsync(a => a.EmergenciaId == emergenciaId && a.Estado == EstadoAsignacion.Activa)
           || await Context.AsignacionesVehiculos.AnyAsync(a => a.EmergenciaId == emergenciaId && a.Estado == EstadoAsignacion.Activa);

    public Task<int> ContarAsync(DateTime? desde = null)
        => Context.Emergencias.CountAsync(e => desde == null || e.FechaHora >= desde);

    public Task<Dictionary<EstadoEmergencia, int>> ContarPorEstadoAsync()
        => Context.Emergencias.GroupBy(e => e.Estado)
            .Select(g => new { g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Total);

    public Task<Dictionary<OrigenEmergencia, int>> ContarPorOrigenAsync()
        => Context.Emergencias.GroupBy(e => e.Origen)
            .Select(g => new { g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Total);

    public Task<List<Emergencia>> GetUltimasAsync(int cantidad)
        => Context.Emergencias.AsNoTracking().OrderByDescending(e => e.FechaHora).Take(cantidad).ToListAsync();
}

public class AsignacionRepository(ApplicationContext context) : IAsignacionRepository
{
    public Task<AsignacionBombero?> GetBomberoAsync(int emergenciaId, int bomberoId)
        => context.AsignacionesBomberos.Include(a => a.Bombero)
            .FirstOrDefaultAsync(a => a.EmergenciaId == emergenciaId && a.BomberoId == bomberoId);

    public Task<AsignacionVehiculo?> GetVehiculoAsync(int emergenciaId, int vehiculoId)
        => context.AsignacionesVehiculos.Include(a => a.Vehiculo)
            .FirstOrDefaultAsync(a => a.EmergenciaId == emergenciaId && a.VehiculoId == vehiculoId);

    public Task<bool> BomberoTieneAsignacionActivaAsync(int bomberoId)
        => context.AsignacionesBomberos.AnyAsync(a => a.BomberoId == bomberoId && a.Estado == EstadoAsignacion.Activa);

    public Task<bool> VehiculoTieneAsignacionActivaAsync(int vehiculoId)
        => context.AsignacionesVehiculos.AnyAsync(a => a.VehiculoId == vehiculoId && a.Estado == EstadoAsignacion.Activa);

    public async Task AddBomberoAsync(AsignacionBombero asignacion)
    {
        context.AsignacionesBomberos.Add(asignacion);
        await context.SaveChangesAsync();
    }

    public async Task AddVehiculoAsync(AsignacionVehiculo asignacion)
    {
        context.AsignacionesVehiculos.Add(asignacion);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBomberoAsync(AsignacionBombero asignacion)
    {
        context.Entry(asignacion).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task UpdateVehiculoAsync(AsignacionVehiculo asignacion)
    {
        context.Entry(asignacion).State = EntityState.Modified;
        await context.SaveChangesAsync();
    }

    public async Task<HashSet<int>> GetBomberosEnServicioAsync()
        => (await context.AsignacionesBomberos
            .Where(a => a.Estado == EstadoAsignacion.Activa)
            .Select(a => a.BomberoId)
            .ToListAsync()).ToHashSet();

    public Task<List<AsignacionBombero>> ListarBomberosAsync(EstadoAsignacion? estado)
        => context.AsignacionesBomberos.AsNoTracking().Include(a => a.Bombero)
            .Where(a => estado == null || a.Estado == estado)
            .OrderByDescending(a => a.FechaAsignacion)
            .ToListAsync();

    public Task<List<AsignacionVehiculo>> ListarVehiculosAsync(EstadoAsignacion? estado)
        => context.AsignacionesVehiculos.AsNoTracking().Include(a => a.Vehiculo)
            .Where(a => estado == null || a.Estado == estado)
            .OrderByDescending(a => a.FechaAsignacion)
            .ToListAsync();
}

public class LogRepository(ApplicationContext context) : ILogRepository
{
    public Task<List<AuditLog>> BuscarAuditoriaAsync(LogFiltroDto filtro)
    {
        var query = context.AuditLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filtro.UsuarioId)) query = query.Where(l => l.UsuarioId == filtro.UsuarioId);
        if (!string.IsNullOrWhiteSpace(filtro.Accion)) query = query.Where(l => l.Accion.Contains(filtro.Accion));
        if (filtro.Desde is { } desde) query = query.Where(l => l.FechaHora >= desde);
        if (Fechas.FinInclusivo(filtro.Hasta) is { } hasta) query = query.Where(l => l.FechaHora < hasta);
        return query.OrderByDescending(l => l.Id).Take(filtro.Limite).ToListAsync();
    }

    public Task<List<RequestLog>> BuscarPeticionesAsync(LogFiltroDto filtro)
    {
        var query = context.RequestLogs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filtro.UsuarioId)) query = query.Where(l => l.UsuarioId == filtro.UsuarioId);
        if (!string.IsNullOrWhiteSpace(filtro.Accion)) query = query.Where(l => l.Endpoint.Contains(filtro.Accion));
        if (filtro.Desde is { } desde) query = query.Where(l => l.FechaHora >= desde);
        if (Fechas.FinInclusivo(filtro.Hasta) is { } hasta) query = query.Where(l => l.FechaHora < hasta);
        return query.OrderByDescending(l => l.Id).Take(filtro.Limite).ToListAsync();
    }
}

internal static class Fechas
{
    /// <summary>Un "hasta" sin hora (solo fecha) incluye el día completo.</summary>
    public static DateTime? FinInclusivo(DateTime? hasta)
        => hasta is { } h && h.TimeOfDay == TimeSpan.Zero ? h.AddDays(1) : hasta;
}
