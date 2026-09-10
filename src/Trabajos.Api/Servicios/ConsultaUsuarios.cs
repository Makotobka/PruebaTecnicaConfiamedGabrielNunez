using Trabajos.Api.Modelos;

namespace Trabajos.Api.Servicios;

/// <summary>
/// Consulta el microservicio de usuarios mediante HTTP.
/// </summary>
public class ConsultaUsuarios(HttpClient cliente) : IConsultaUsuarios
{
    /// <inheritdoc />
    public async Task<List<UsuarioReferencia>> ListarAsync()
    {
        return await cliente.GetFromJsonAsync<List<UsuarioReferencia>>("api/usuarios")
            ?? throw new HttpRequestException("No se pudo obtener la lista de usuarios.");
    }

    /// <inheritdoc />
    public async Task<bool> ExisteNickAsync(string nick)
    {
        var ruta = $"api/usuarios/existe?nick={Uri.EscapeDataString(nick)}";
        return await cliente.GetFromJsonAsync<bool>(ruta);
    }

    /// <inheritdoc />
    public async Task<bool> ExisteNombreAsync(string nombre)
    {
        var ruta = $"api/usuarios/existe?nombre={Uri.EscapeDataString(nombre)}";
        return await cliente.GetFromJsonAsync<bool>(ruta);
    }
}
