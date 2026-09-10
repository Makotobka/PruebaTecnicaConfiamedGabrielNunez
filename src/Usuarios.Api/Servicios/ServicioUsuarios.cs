using System.ComponentModel.DataAnnotations;
using Usuarios.Api.Modelos;
using Usuarios.Api.Repositorios;

namespace Usuarios.Api.Servicios;

/// <summary>
/// Aplica las validaciones de negocio y coordina la persistencia de usuarios.
/// </summary>
public class ServicioUsuarios(IRepositorioUsuarios repositorio) : IServicioUsuarios
{
    public Task<List<Usuario>> ListarAsync()
        => repositorio.ListarAsync();

    public async Task<Usuario> ObtenerAsync(Guid id)
        => await repositorio.ObtenerAsync(id)
            ?? throw new KeyNotFoundException();

    public async Task<Usuario> CrearAsync(SolicitudUsuario solicitud)
    {
        var usuario = Preparar(solicitud);
        return await repositorio.CrearAsync(usuario);
    }

    public async Task ActualizarAsync(Guid id, SolicitudUsuario solicitud)
    {
        var usuario = Preparar(solicitud);
        usuario.Id = id;
        if (!await repositorio.ActualizarAsync(usuario))
            throw new KeyNotFoundException();
    }

    public async Task EliminarAsync(Guid id)
    {
        if (!await repositorio.EliminarAsync(id))
            throw new KeyNotFoundException();
    }

    public Task<bool> ExisteNickAsync(string nick)
    {
        if (string.IsNullOrWhiteSpace(nick) || nick.Length > 100)
            throw new ValidationException("El nick debe tener entre 1 y 100 caracteres.");
        return repositorio.ExisteNickAsync(nick.Trim());
    }

    public Task<bool> ExisteUserAsync(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > 100)
            throw new ValidationException("El nombre debe tener entre 1 y 100 caracteres.");
        return repositorio.ExisteUserAsync (nombre.Trim());
    }

    

    /// <summary>Valida y normaliza los datos recibidos antes de guardarlos.</summary>
    private static Usuario Preparar(SolicitudUsuario solicitud)
    {
        Validator.ValidateObject(solicitud, new ValidationContext(solicitud), validateAllProperties: true);
        return new Usuario
        {
            Nick = solicitud.Nick.Trim(),
            Nombre = solicitud.Nombre.Trim()
        };
    }
}
