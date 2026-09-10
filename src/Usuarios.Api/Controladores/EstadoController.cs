using Microsoft.AspNetCore.Mvc;

namespace Usuarios.Api.Controladores;

[ApiController]
[Route("estado")]
/// <summary>
/// Expone una comprobación básica de disponibilidad del microservicio de usuarios.
/// </summary>
public class EstadoController : ControllerBase
{
    /// <summary>Confirma que la API está ejecutándose y puede responder solicitudes.</summary>
    [HttpGet]
    public IActionResult Obtener()
    {
        return Ok(new { estado = "disponible" });
    }
}
