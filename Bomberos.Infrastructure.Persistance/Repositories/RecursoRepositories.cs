using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Core.Domain.Entities;
using Bomberos.Core.Domain.Enums;
using Bomberos.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.Infrastructure.Persistence.Repositories;

public class EstacionRepository(ApplicationContext context) : GenericRepository<Estacion>(context), IEstacionRepository
{
    public Task<bool> ExisteNombreAsync(string nombre, int? exceptoId = null)
        => Context.Estaciones.AnyAsync(e => e.Nombre == nombre && (exceptoId == null || e.Id != exceptoId));
}

public class BomberoRepository(ApplicationContext context) : GenericRepository<Bombero>(context), IBomberoRepository
{
    public Task<List<Bombero>> GetAllConEstacionAsync()
        => Context.Bomberos.Include(b => b.Estacion).OrderBy(b => b.Apellido).ThenBy(b => b.Nombre).ToListAsync();

    public Task<Bombero?> GetConEstacionAsync(int id)
        => Context.Bomberos.Include(b => b.Estacion).FirstOrDefaultAsync(b => b.Id == id);

    public Task<bool> ExisteCedulaAsync(string cedula, int? exceptoId = null)
        => Context.Bomberos.AnyAsync(b => b.Cedula == cedula && (exceptoId == null || b.Id != exceptoId));

    public Task<List<Bombero>> GetDisponiblesAsync()
        => Context.Bomberos.Include(b => b.Estacion)
            .Where(b => b.Estado == EstadoBombero.Activo
                        && !b.Asignaciones.Any(a => a.Estado == EstadoAsignacion.Activa))
            .OrderBy(b => b.Apellido).ThenBy(b => b.Nombre)
            .ToListAsync();

    public Task<int> ContarActivosAsync() => Context.Bomberos.CountAsync(b => b.Estado == EstadoBombero.Activo);
}

public class VehiculoRepository(ApplicationContext context) : GenericRepository<Vehiculo>(context), IVehiculoRepository
{
    public Task<List<Vehiculo>> GetAllConEstacionAsync()
        => Context.Vehiculos.Include(v => v.Estacion).OrderBy(v => v.Placa).ToListAsync();

    public Task<Vehiculo?> GetConEstacionAsync(int id)
        => Context.Vehiculos.Include(v => v.Estacion).FirstOrDefaultAsync(v => v.Id == id);

    public Task<bool> ExistePlacaAsync(string placa, int? exceptoId = null)
        => Context.Vehiculos.AnyAsync(v => v.Placa == placa && (exceptoId == null || v.Id != exceptoId));

    public Task<List<Vehiculo>> GetDisponiblesAsync()
        => Context.Vehiculos.Include(v => v.Estacion)
            .Where(v => v.Estado == EstadoVehiculo.Disponible)
            .OrderBy(v => v.Placa)
            .ToListAsync();

    public Task<Dictionary<EstadoVehiculo, int>> ContarPorEstadoAsync()
        => Context.Vehiculos.GroupBy(v => v.Estado)
            .Select(g => new { g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Total);
}
