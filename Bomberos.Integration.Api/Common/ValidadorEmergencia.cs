namespace Bomberos.Integration.Api.Common;

/// <summary>
/// Validación de formato de una emergencia antes de enviarla a CORE o de encolarla.
/// Las reglas de negocio las aplica CORE; aquí solo se evita aceptar datos que CORE rechazaría siempre.
/// </summary>
public static class ValidadorEmergencia
{
    public static readonly string[] Tipos = ["INCENDIO", "ACCIDENTE", "RESCATE", "INCIDENTE_PELIGROSO", "OTRO"];
    public static readonly string[] Prioridades = ["BAJA", "MEDIA", "ALTA", "CRITICA"];

    private const int MaxDescripcion = 500;
    private const int MaxDireccion = 200;
    private const int MaxReferencia = 50;

    /// <returns>Lista de errores; vacía si los datos son válidos.</returns>
    public static List<string> Validar(string? tipo, string? prioridad, string? descripcion, string? direccion)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(tipo)) errores.Add("El tipo es requerido.");
        else if (!Tipos.Contains(tipo.Trim(), StringComparer.OrdinalIgnoreCase))
            errores.Add($"El tipo '{tipo}' no es válido. Valores permitidos: {string.Join(", ", Tipos)}.");

        if (string.IsNullOrWhiteSpace(prioridad)) errores.Add("La prioridad es requerida.");
        else if (!Prioridades.Contains(prioridad.Trim(), StringComparer.OrdinalIgnoreCase))
            errores.Add($"La prioridad '{prioridad}' no es válida. Valores permitidos: {string.Join(", ", Prioridades)}.");

        ValidarTexto(errores, "La descripción", descripcion, MaxDescripcion);
        ValidarTexto(errores, "La dirección", direccion, MaxDireccion);
        return errores;
    }

    public static void ValidarReferencia(List<string> errores, string? referencia)
    {
        ValidarTexto(errores, "La referencia", referencia, MaxReferencia);
        if (!string.IsNullOrWhiteSpace(referencia) && referencia.Trim().Any(c => char.IsWhiteSpace(c) || char.IsControl(c) || c is '/' or '\\' or '?' or '#' or '%'))
            errores.Add("La referencia no admite espacios ni los caracteres / \\ ? # %.");
    }

    private static void ValidarTexto(List<string> errores, string campo, string? valor, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor)) errores.Add($"{campo} es requerida.");
        else if (valor.Trim().Length > maximo) errores.Add($"{campo} admite un máximo de {maximo} caracteres.");
    }
}
