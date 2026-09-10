using Trabajos.Api.Modelos;

namespace Trabajos.Api.Repositorios;

public interface IRepositorioItemsTrabajo
{
    Task<List<ItemTrabajo>> ListarAsync();
    Task<ItemTrabajo?> ObtenerAsync(Guid id);
    Task<ItemTrabajo> CrearAsync(ItemTrabajo entidad);
    Task<bool> ActualizarAsync(ItemTrabajo entidad);
    Task<bool> EliminarAsync(Guid id);
}
