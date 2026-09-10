using Microsoft.AspNetCore.Mvc;
using Trabajos.Api.Modelos;
using Trabajos.Api.Servicios;

namespace Trabajos.Api.Controladores;

[ApiController]
[Route("api/items-trabajo")]
/// <summary>
/// Expone las operaciones HTTP para administrar y consultar ítems de trabajo.
/// </summary>
public class ItemsTrabajoController(IServicioItemsTrabajo servicio) : ControllerBase
{
    /// <summary>Obtiene todos los ítems en el orden definido por las reglas de negocio.</summary>
    [HttpGet]
    public async Task<ActionResult<List<ItemTrabajo>>> Listar()
    {
        return Ok(await servicio.ListarAsync());
    }

    /// <summary>Obtiene los pendientes ordenados de un usuario.</summary>
    [HttpGet("pendientes")]
    public async Task<ActionResult<List<ItemTrabajo>>> ListarPendientes([FromQuery] string nombreUsuario)
    {
        return Ok(await servicio.ListarPendientesAsync(nombreUsuario));
    }

    /// <summary>Obtiene un ítem por su identificador.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemTrabajo>> Obtener(Guid id)
    {
        return Ok(await servicio.ObtenerAsync(id));
    }

    /// <summary>Crea un ítem y asigna automáticamente un usuario disponible.</summary>
    [HttpPost]
    public async Task<ActionResult<ItemTrabajo>> Crear(SolicitudItemTrabajo solicitud)
    {
        var entidad = await servicio.CrearAsync(solicitud);
        return CreatedAtAction(nameof(Obtener), new { id = entidad.IdItem }, entidad);
    }

    /// <summary>Actualiza los datos editables de un ítem existente.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Actualizar(Guid id, SolicitudItemTrabajo solicitud)
    {
        await servicio.ActualizarAsync(id, solicitud);
        return NoContent();
    }

    /// <summary>Elimina definitivamente un ítem.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        await servicio.EliminarAsync(id);
        return NoContent();
    }
}
