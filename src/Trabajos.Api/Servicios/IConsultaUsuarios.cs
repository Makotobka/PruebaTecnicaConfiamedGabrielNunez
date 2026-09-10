using Trabajos.Api.Modelos;

namespace Trabajos.Api.Servicios;

/// <summary>
/// Define las consultas realizadas al microservicio de usuarios.
/// </summary>
public interface IConsultaUsuarios
{
    /// <summary>Obtiene los usuarios disponibles para la distribución de trabajo.</summary>
    Task<List<UsuarioReferencia>> ListarAsync();
    /// <summary>Indica si existe un usuario con el nick proporcionado.</summary>
    Task<bool> ExisteNickAsync(string nick);
    /// <summary>Indica si existe un usuario con el nombre proporcionado.</summary>
    Task<bool> ExisteNombreAsync(string nombre);
}
