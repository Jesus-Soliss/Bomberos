using System.Text;

namespace Bomberos.Core.Domain.Common;

/// <summary>
/// Convierte los enums al texto que define el documento funcional
/// (EnServicio ⇄ EN_SERVICIO, Central911 ⇄ CENTRAL_911). Se usa tanto en la base de datos como en el JSON.
/// </summary>
public static class EnumTexto
{
    public static string ToTexto<TEnum>(this TEnum valor) where TEnum : struct, Enum
        => Convertir(valor.ToString());

    /// <summary>Texto para guardar en la base de datos. Falla si el valor no está definido en el enum.</summary>
    public static string ToTextoValido<TEnum>(TEnum valor) where TEnum : struct, Enum
        => Enum.IsDefined(valor)
            ? Convertir(valor.ToString())
            : throw new ArgumentOutOfRangeException(nameof(valor), valor, $"Valor no definido en {typeof(TEnum).Name}.");

    public static string Convertir(string nombre)
    {
        var sb = new StringBuilder(nombre.Length + 4);
        for (var i = 0; i < nombre.Length; i++)
        {
            var c = nombre[i];
            if (i > 0)
            {
                var anterior = nombre[i - 1];
                var nuevaPalabra = (char.IsUpper(c) && char.IsLower(anterior))
                                   || (char.IsDigit(c) && char.IsLetter(anterior));
                if (nuevaPalabra) sb.Append('_');
            }
            sb.Append(char.ToUpperInvariant(c));
        }
        return sb.ToString();
    }

    public static bool TryParse<TEnum>(string? texto, out TEnum valor) where TEnum : struct, Enum
    {
        valor = default;
        if (string.IsNullOrWhiteSpace(texto)) return false;
        return Enum.TryParse(texto.Replace("_", ""), ignoreCase: true, out valor) && Enum.IsDefined(valor);
    }

    public static TEnum Parse<TEnum>(string texto) where TEnum : struct, Enum
        => TryParse<TEnum>(texto, out var valor)
            ? valor
            : throw new ArgumentException($"'{texto}' no es un valor válido de {typeof(TEnum).Name}.");
}
