using Microsoft.AspNetCore.Mvc;

namespace Usuarios.Api.Controladores;

[ApiController]
[Route("estado")]
public class EstadoController : ControllerBase
{
    [HttpGet]
    public IActionResult Obtener()
    {
        return Ok(new { estado = "disponible" });
    }
}
