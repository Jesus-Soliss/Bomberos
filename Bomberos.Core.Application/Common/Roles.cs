namespace Bomberos.Core.Application.Common;

public static class Roles
{
    public const string Administrador = "ADMINISTRADOR";
    public const string Operador = "OPERADOR";
    public const string Consulta = "CONSULTA";
    /// <summary>Usuario externo que se registra por su cuenta: solo puede registrar emergencias.</summary>
    public const string Cliente = "CLIENTE";

    // Para [Authorize(Roles = ...)]
    public const string AdministradorOperador = Administrador + "," + Operador;
    public const string RegistranEmergencias = Administrador + "," + Operador + "," + Cliente;

    public static readonly string[] Base = [Administrador, Operador, Consulta, Cliente];
}

public static class Politicas
{
    /// <summary>Personal del sistema: cualquier usuario autenticado que no sea CLIENTE.</summary>
    public const string Personal = "Personal";
}

public static class EstadosUsuario
{
    public const string Activo = "ACTIVO";
    public const string Inactivo = "INACTIVO";
}
