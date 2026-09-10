using Microsoft.AspNetCore.Mvc;
using Trabajos.Api.Modelos;
using Trabajos.Api.Servicios;

namespace Trabajos.Api.Controladores;

[ApiController]
[Route("api/items-trabajo")]
public class ItemsTrabajoController(IServicioItemsTrabajo servicio) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ItemTrabajo>>> Listar()
    {
        return Ok(await servicio.ListarAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemTrabajo>> Obtener(Guid id)
    {
        return Ok(await servicio.ObtenerAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<ItemTrabajo>> Crear(SolicitudItemTrabajo solicitud)
    {
        var entidad = await servicio.CrearAsync(solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = entidad.IdItem }, entidad);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, SolicitudItemTrabajo solicitud)
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
