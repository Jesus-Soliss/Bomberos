using System.Text.Json;
using System.Text.Json.Serialization;
using Bomberos.Core.Domain.Common;

namespace Bomberos.Core.Application.Common;

/// <summary>Los enums viajan en el JSON con el texto del documento funcional (EN_SERVICIO, CENTRAL_911, ...).</summary>
public class EnumTextoNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => EnumTexto.Convertir(name);
}

public static class ApiJson
{
    /// <summary>Opciones con las que la WebApi escribe su JSON; la WebApp las usa para leerlo.</summary>
    public static readonly JsonSerializerOptions Options = Crear();

    public static JsonStringEnumConverter EnumConverter() => new(new EnumTextoNamingPolicy());

    private static JsonSerializerOptions Crear()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(EnumConverter());
        return options;
    }
}
