using Usuarios.Api.Modelos;

namespace Usuarios.Api.Repositorios;

public interface IRepositorioUsuarios
{
    Task<List<Usuario>> ListarAsync();
    Task<Usuario?> ObtenerAsync(Guid id);
    Task<Usuario> CrearAsync(Usuario entidad);
    Task<bool> ActualizarAsync(Usuario entidad);
    Task<bool> EliminarAsync(Guid id);
    Task<bool> ExisteNickAsync(string nick);
    Task<bool> ExisteUserAsync(string nombre);
}
