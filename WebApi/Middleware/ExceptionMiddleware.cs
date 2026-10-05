using Bomberos.Core.Application.Exceptions;
using Bomberos.WebApi.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Bomberos.WebApi.Middleware;

/// <summary>Convierte las excepciones en respuestas JSON uniformes.</summary>
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex) when (!ctx.Response.HasStarted)
        {
            var (status, message) = ex switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
                ConflictException => (StatusCodes.Status409Conflict, ex.Message),
                BusinessException => (StatusCodes.Status400BadRequest, ex.Message),
                // Índice único violado por dos peticiones simultáneas (RN-03, RN-04, RN-13).
                DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } }
                    => (StatusCodes.Status409Conflict, "La operación entra en conflicto con un registro existente."),
                _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error inesperado.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Error no controlado en {Method} {Path}", ctx.Request.Method, ctx.Request.Path);

            ctx.Response.StatusCode = status;
            await ctx.Response.WriteAsJsonAsync(new ApiError(message));
        }
    }
}
