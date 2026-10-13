using AutoMapper;
using Bomberos.Core.Application.DTOs.Auth;
using Bomberos.Core.Application.DTOs.Emergencias;
using Bomberos.Core.Application.DTOs.Recursos;
using Bomberos.Core.Application.DTOs.Usuarios;
using Bomberos.Core.Application.ViewModels;

namespace Bomberos.Core.Application.Mappings;

/// <summary>ViewModels de la WebApp ⇄ DTOs que viajan a la API.</summary>
public class ViewModelProfile : Profile
{
    public ViewModelProfile()
    {
        CreateMap<LoginViewModel, AuthRequest>();
        CreateMap<RegistroViewModel, RegistroClienteDto>();

        CreateMap<UsuarioCrearViewModel, CrearUsuarioDto>();
        CreateMap<UsuarioEditarViewModel, EditarUsuarioDto>();
        CreateMap<UsuarioDto, UsuarioEditarViewModel>();
        CreateMap<UsuarioDto, RestablecerPasswordViewModel>()
            .ForMember(d => d.NuevaPassword, o => o.Ignore())
            .ForMember(d => d.ConfirmarPassword, o => o.Ignore());
        CreateMap<RolViewModel, GuardarRolDto>();
        CreateMap<RolDto, RolViewModel>();

        CreateMap<EstacionViewModel, GuardarEstacionDto>();
        CreateMap<EstacionDto, EstacionViewModel>();
        CreateMap<BomberoViewModel, GuardarBomberoDto>();
        CreateMap<BomberoDto, BomberoViewModel>();
        CreateMap<VehiculoViewModel, GuardarVehiculoDto>();
        CreateMap<VehiculoDto, VehiculoViewModel>();
        CreateMap<EmergenciaViewModel, GuardarEmergenciaDto>();
        CreateMap<EmergenciaDto, EmergenciaViewModel>();
    }
}
