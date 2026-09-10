using Trabajos.Api.Modelos;

namespace Trabajos.Api.Servicios;

public interface IConsultaUsuarios
{
    Task<List<UsuarioReferencia>> ListarAsync();
    Task<bool> ExisteNickAsync(string nick);
    Task<bool> ExisteNombreAsync(string nombre);
}
