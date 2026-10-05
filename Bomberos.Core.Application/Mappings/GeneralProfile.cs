using AutoMapper;
using Bomberos.Core.Application.DTOs.Consultas;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.DTOs.Recursos;
using Bomberos.Core.Domain.Entities;

namespace Bomberos.Core.Application.Mappings;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        CreateMap<Estacion, EstacionDto>();
        CreateMap<GuardarEstacionDto, Estacion>();

        CreateMap<Bombero, BomberoDto>()
            .ForMember(d => d.EstacionNombre, o => o.MapFrom(s => s.Estacion.Nombre))
            .ForMember(d => d.EnServicio, o => o.Ignore());
        CreateMap<GuardarBomberoDto, Bombero>();

        CreateMap<Vehiculo, VehiculoDto>()
            .ForMember(d => d.EstacionNombre, o => o.MapFrom(s => s.Estacion.Nombre));
        CreateMap<GuardarVehiculoDto, Vehiculo>();

        CreateMap<Emergencia, EmergenciaDto>();
        CreateMap<Emergencia, EmergenciaDetalleDto>()
            .ForMember(d => d.Bomberos, o => o.MapFrom(s => s.AsignacionesBomberos.OrderBy(a => a.FechaAsignacion)))
            .ForMember(d => d.Vehiculos, o => o.MapFrom(s => s.AsignacionesVehiculos.OrderBy(a => a.FechaAsignacion)));
        CreateMap<GuardarEmergenciaDto, Emergencia>();

        CreateMap<AsignacionBombero, AsignacionBomberoDto>()
            .ForMember(d => d.BomberoNombre, o => o.MapFrom(s => s.Bombero.Nombre + " " + s.Bombero.Apellido))
            .ForMember(d => d.Cedula, o => o.MapFrom(s => s.Bombero.Cedula))
            .ForMember(d => d.Rango, o => o.MapFrom(s => s.Bombero.Rango));
        CreateMap<AsignacionVehiculo, AsignacionVehiculoDto>()
            .ForMember(d => d.Placa, o => o.MapFrom(s => s.Vehiculo.Placa))
            .ForMember(d => d.TipoVehiculo, o => o.MapFrom(s => s.Vehiculo.Tipo));

        CreateMap<AuditLog, AuditLogDto>();
        CreateMap<RequestLog, RequestLogDto>();
    }
}
