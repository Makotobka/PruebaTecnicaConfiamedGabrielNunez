using Usuarios.Api.Modelos;

namespace Usuarios.Api.Repositorios;

/// <summary>
/// Define el acceso a los usuarios almacenados en SQL Server.
/// </summary>
public interface IRepositorioUsuarios
{
    /// <summary>Recupera todos los usuarios registrados.</summary>
    Task<List<Usuario>> ListarAsync();
    /// <summary>Busca un usuario por su identificador; devuelve nulo cuando no existe.</summary>
    Task<Usuario?> ObtenerAsync(Guid id);
    /// <summary>Inserta un usuario y devuelve la entidad con el identificador generado.</summary>
    Task<Usuario> CrearAsync(Usuario entidad);
    /// <summary>Actualiza un usuario y señala si se encontró el registro.</summary>
    Task<bool> ActualizarAsync(Usuario entidad);
    /// <summary>Elimina un usuario y señala si se encontró el registro.</summary>
    Task<bool> EliminarAsync(Guid id);
    /// <summary>Indica si existe un usuario con el nick proporcionado.</summary>
    Task<bool> ExisteNickAsync(string nick);
    /// <summary>Indica si existe un usuario con el nombre proporcionado.</summary>
    Task<bool> ExisteUserAsync(string nombre);
}
