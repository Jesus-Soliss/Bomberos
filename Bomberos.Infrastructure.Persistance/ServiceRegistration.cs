using Bomberos.Core.Application.Interfaces.Repositories;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Infrastructure.Persistence.Contexts;
using Bomberos.Infrastructure.Persistence.Repositories;
using Bomberos.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bomberos.Infrastructure.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationContext>(o => o.UseSqlServer(
            config.GetConnectionString("DefaultConnection"),
            m => m.MigrationsAssembly(typeof(ApplicationContext).Assembly.FullName)));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEstacionRepository, EstacionRepository>();
        services.AddScoped<IBomberoRepository, BomberoRepository>();
        services.AddScoped<IVehiculoRepository, VehiculoRepository>();
        services.AddScoped<IEmergenciaRepository, EmergenciaRepository>();
        services.AddScoped<IAsignacionRepository, AsignacionRepository>();
        services.AddScoped<ILogRepository, LogRepository>();
        services.AddScoped<IAuditService, AuditService>();
    }
}
