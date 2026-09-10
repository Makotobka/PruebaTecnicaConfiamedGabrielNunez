using Microsoft.AspNetCore.Mvc;
using Usuarios.Api.Modelos;
using Usuarios.Api.Servicios;

namespace Usuarios.Api.Controladores;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(IServicioUsuarios servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Usuario>>> Listar()
    {
        return Ok(await servicio.ListarAsync());
    }

    [HttpGet("existe")]
    public async Task<ActionResult<bool>> ExisteNick([FromQuery] string? nick, [FromQuery] string? nombre)
    {
        if(!string.IsNullOrEmpty(nick))
            return Ok(await servicio.ExisteNickAsync(nick));
        if(!string.IsNullOrEmpty(nombre))
            return Ok(await servicio.ExisteUserAsync(nombre));
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Usuario>> Obtener(Guid id)
    {
        return Ok(await servicio.ObtenerAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<Usuario>> Crear(SolicitudUsuario solicitud)
    {
        var entidad = await servicio.CrearAsync(solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = entidad.Id }, entidad);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, SolicitudUsuario solicitud)
    {
        await servicio.ActualizarAsync(id, solicitud);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
