using Usuarios.Api.Modelos;

namespace Usuarios.Api.Servicios;

/// <summary>
/// Define las operaciones de negocio disponibles para administrar usuarios.
/// </summary>
public interface IServicioUsuarios
{
    /// <summary>Lista todos los usuarios registrados.</summary>
    Task<List<Usuario>> ListarAsync();
    /// <summary>Obtiene un usuario mediante su identificador.</summary>
    Task<Usuario> ObtenerAsync(Guid id);
    /// <summary>Valida y crea un usuario.</summary>
    Task<Usuario> CrearAsync(SolicitudUsuario solicitud);
    /// <summary>Valida y actualiza un usuario existente.</summary>
    Task ActualizarAsync(Guid id, SolicitudUsuario solicitud);
    /// <summary>Elimina un usuario mediante su identificador.</summary>
    Task EliminarAsync(Guid id);
    /// <summary>Indica si existe un usuario con el nick proporcionado.</summary>
    Task<bool> ExisteNickAsync(string nick);
    /// <summary>Indica si existe un usuario con el nombre proporcionado.</summary>
    Task<bool> ExisteUserAsync(string nombre);
}
