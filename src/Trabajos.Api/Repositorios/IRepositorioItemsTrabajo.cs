using Trabajos.Api.Modelos;

namespace Trabajos.Api.Repositorios;

/// <summary>
/// Define el acceso a los ítems de trabajo almacenados en SQL Server.
/// </summary>
public interface IRepositorioItemsTrabajo
{
    /// <summary>Recupera todos los ítems registrados.</summary>
    Task<List<ItemTrabajo>> ListarAsync();
    /// <summary>Busca un ítem por su identificador; devuelve nulo cuando no existe.</summary>
    Task<ItemTrabajo?> ObtenerAsync(Guid id);
    /// <summary>Inserta un ítem y devuelve la entidad con el identificador generado.</summary>
    Task<ItemTrabajo> CrearAsync(ItemTrabajo entidad);
    /// <summary>Actualiza un ítem y señala si se encontró el registro.</summary>
    Task<bool> ActualizarAsync(ItemTrabajo entidad);
    /// <summary>Elimina un ítem y señala si se encontró el registro.</summary>
    Task<bool> EliminarAsync(Guid id);
}
