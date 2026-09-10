using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Trabajos.Api.Errores;

public class ManejadorErrores(RequestDelegate siguiente)
{
    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await siguiente(contexto);
        }
        catch (Exception excepcion) when (!contexto.Response.HasStarted)
        {
            contexto.Response.Clear();
            await ManejarAsync(contexto, excepcion);
        }
    }

    private static async Task ManejarAsync(HttpContext contexto, Exception excepcion)
    {
        var (codigo, titulo) = excepcion switch
        {
            ValidationException => (400, "Los datos enviados no son válidos."),
            KeyNotFoundException => (404, "El registro no existe."),
            SqlException or HttpRequestException or TaskCanceledException => (503, "Un servicio necesario no está disponible."),
            Exception => (510, "Error no controlado internamente"),
            _ => (500, "Ocurrió un error al procesar la solicitud.")
        };

        contexto.Response.StatusCode = codigo;
        await contexto.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = codigo,
            Title = titulo,
            Instance = excepcion?.StackTrace,
            Detail = codigo == 400 ? excepcion?.Message : null
        });
    }
}
