using Usuarios.Api.Modelos;

namespace Usuarios.Api.Servicios;

public interface IServicioUsuarios
{
    Task<List<Usuario>> ListarAsync();
    Task<Usuario> ObtenerAsync(Guid id);
    Task<Usuario> CrearAsync(SolicitudUsuario solicitud);
    Task ActualizarAsync(Guid id, SolicitudUsuario solicitud);
    Task EliminarAsync(Guid id);
    Task<bool> ExisteNickAsync(string nick);
    Task<bool> ExisteUserAsync(string nombre);
}
