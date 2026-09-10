using Trabajos.Api.Modelos;

namespace Trabajos.Api.Servicios;

/// <summary>
/// Define las operaciones de negocio disponibles para administrar ítems de trabajo.
/// </summary>
public interface IServicioItemsTrabajo
{
    /// <summary>Lista los ítems agrupados y ordenados por usuario, estado, relevancia y entrega.</summary>
    Task<List<ItemTrabajo>> ListarAsync();
    /// <summary>Lista los pendientes de un usuario priorizando relevancia y fecha de entrega.</summary>
    Task<List<ItemTrabajo>> ListarPendientesAsync(string nombreUsuario);
    /// <summary>Obtiene un ítem mediante su identificador.</summary>
    Task<ItemTrabajo> ObtenerAsync(Guid id);
    /// <summary>Valida, asigna y crea un ítem de trabajo.</summary>
    Task<ItemTrabajo> CrearAsync(SolicitudItemTrabajo solicitud);
    /// <summary>Valida y actualiza un ítem existente.</summary>
    Task ActualizarAsync(Guid id, SolicitudItemTrabajo solicitud);
    /// <summary>Elimina un ítem mediante su identificador.</summary>
    Task EliminarAsync(Guid id);
}
