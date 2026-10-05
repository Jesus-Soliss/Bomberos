namespace Bomberos.Core.Application.Common;

public static class Roles
{
    public const string Administrador = "ADMINISTRADOR";
    public const string Operador = "OPERADOR";
    public const string Consulta = "CONSULTA";

    // Para [Authorize(Roles = ...)]
    public const string AdministradorOperador = Administrador + "," + Operador;

    public static readonly string[] Base = [Administrador, Operador, Consulta];
}

public static class EstadosUsuario
{
    public const string Activo = "ACTIVO";
    public const string Inactivo = "INACTIVO";
}
