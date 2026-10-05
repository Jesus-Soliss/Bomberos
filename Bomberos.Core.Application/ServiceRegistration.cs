using System.Reflection;
using Bomberos.Core.Application.Interfaces.Services;
using Bomberos.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bomberos.Core.Application;

public static class ServiceRegistration
{
    /// <param name="autoMapperLicenseKey">Clave de licencia de AutoMapper (opcional; hay una gratuita de comunidad).</param>
    public static void AddApplicationLayer(this IServiceCollection services, string? autoMapperLicenseKey = null)
    {
        services.AddAutoMapper(cfg =>
        {
            if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey)) cfg.LicenseKey = autoMapperLicenseKey;
        }, Assembly.GetExecutingAssembly());

        services.AddScoped<IEstacionService, EstacionService>();
        services.AddScoped<IBomberoService, BomberoService>();
        services.AddScoped<IVehiculoService, VehiculoService>();
        services.AddScoped<IEmergenciaService, EmergenciaService>();
        services.AddScoped<IAsignacionService, AsignacionService>();
        services.AddScoped<IConsultaService, ConsultaService>();
    }
}
