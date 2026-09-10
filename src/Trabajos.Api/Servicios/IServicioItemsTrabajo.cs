using Trabajos.Api.Modelos;

namespace Trabajos.Api.Servicios;

public interface IServicioItemsTrabajo
{
    Task<List<ItemTrabajo>> ListarAsync();
    Task<ItemTrabajo> ObtenerAsync(Guid id);
    Task<ItemTrabajo> CrearAsync(SolicitudItemTrabajo solicitud);
    Task ActualizarAsync(Guid id, SolicitudItemTrabajo solicitud);
    Task EliminarAsync(Guid id);
}
