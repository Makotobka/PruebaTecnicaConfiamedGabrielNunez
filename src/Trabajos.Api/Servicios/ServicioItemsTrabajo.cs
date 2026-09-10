using System.ComponentModel.DataAnnotations;
using Trabajos.Api.Modelos;
using Trabajos.Api.Repositorios;

namespace Trabajos.Api.Servicios;

/// <summary>
/// Aplica las reglas de validación, asignación y ordenamiento de los ítems de trabajo.
/// </summary>
public class ServicioItemsTrabajo(
    IRepositorioItemsTrabajo repositorio,
    IConsultaUsuarios consultaUsuarios) : IServicioItemsTrabajo
{
    public async Task<List<ItemTrabajo>> ListarAsync()
        => OrdenarItems(await repositorio.ListarAsync());

    public async Task<List<ItemTrabajo>> ListarPendientesAsync(string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            throw new ValidationException("El nombre del usuario es obligatorio.");

        var items = await repositorio.ListarAsync();
        var pendientes = items.Where(item =>
            item.Estado == "Pendiente" &&
            item.NombreUsuarioAsignado == nombreUsuario
        );

        return OrdenarItems(pendientes);
    }

    /// <summary>
    /// Ordena los ítems por usuario y coloca primero los pendientes de mayor relevancia
    /// y fecha de entrega más próxima. El identificador resuelve empates de forma estable.
    /// </summary>
    private static List<ItemTrabajo> OrdenarItems(IEnumerable<ItemTrabajo> items)
        => items
            .OrderBy(item => item.NombreUsuarioAsignado?.Trim(), StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(item => item.Estado == "Pendiente")
            .ThenByDescending(item => item.Relevancia == "Alta")
            .ThenBy(item => item.FechaEntrega)
            .ThenBy(item => item.IdItem)
            .ToList();

    public async Task<ItemTrabajo> ObtenerAsync(Guid id)
        => await repositorio.ObtenerAsync(id)
            ?? throw new KeyNotFoundException();

    public async Task<ItemTrabajo> CrearAsync(SolicitudItemTrabajo solicitud)
    {
        var item = new ItemTrabajo { FechaCreacion = DateTime.Now };
        await AplicarSolicitudAsync(item, solicitud);
        return await repositorio.CrearAsync(item);
    }

    public async Task ActualizarAsync(Guid id, SolicitudItemTrabajo solicitud)
    {
        var item = await ObtenerAsync(id);
        await AplicarSolicitudAsync(item, solicitud);
        if (!await repositorio.ActualizarAsync(item))
            throw new KeyNotFoundException();
    }

    public async Task EliminarAsync(Guid id)
    {
        if (!await repositorio.EliminarAsync(id))
            throw new KeyNotFoundException();
    }

    /// <summary>
    /// Valida la solicitud y completa la asignación y las fechas administradas por el servicio.
    /// </summary>
    private async Task AplicarSolicitudAsync(ItemTrabajo item, SolicitudItemTrabajo solicitud)
    {
        Validator.ValidateObject(solicitud, new ValidationContext(solicitud), validateAllProperties: true);
        var nombre = string.IsNullOrWhiteSpace(solicitud.NombreUsuarioAsignado)
            ? null : solicitud.NombreUsuarioAsignado.Trim();

        var nick = string.IsNullOrWhiteSpace(solicitud.NickUsuarioAsignado)
            ? null : solicitud.NickUsuarioAsignado.Trim();

        if (string.IsNullOrEmpty(nick) && string.IsNullOrEmpty(nombre))
            throw new ValidationException("Se necesita un usuario para asignar el item.");

        if (solicitud.Estado == "Completado")
            throw new ValidationException("Un ítem completado debe tener un usuario asignado.");

        bool UsuarioExiste = true;

        //if (!string.IsNullOrEmpty(nick))
        //    UsuarioExiste &= await consultaUsuarios.ExisteNickAsync(nick);


        if (!string.IsNullOrEmpty(nombre))
            UsuarioExiste &= await consultaUsuarios.ExisteNombreAsync(nombre);

        if (!UsuarioExiste)
            throw new ValidationException("El usuario asignado no existe.");

        item.Titulo = solicitud.Titulo.Trim();
        item.FechaEntrega = solicitud.FechaEntrega ?? DateTime.Now;
        item.Relevancia = solicitud.Relevancia;
        item.Estado = solicitud.Estado;
        item.FechaAsignacion = DateTime.Now;
        item.NombreUsuarioAsignado = await ObtenerUsuarioDisponible(item.FechaEntrega, item.Relevancia, nombre ?? string.Empty);
        item.FechaCompletado = solicitud.Estado == "Completado"
            ? item.FechaCompletado ?? DateTime.Now : null;
    }

    /// <summary>
    /// Selecciona el usuario con menor carga que cumple el límite de ítems de relevancia alta.
    /// Para entregas próximas compara los pendientes sin considerar la relevancia del nuevo ítem.
    /// </summary>
    private async Task<string> ObtenerUsuarioDisponible(DateTime fechaEntrega, string relevancia, string usuarioActual)
    {
        var listaTrabajo = await repositorio.ListarAsync();
        var usuarios = await consultaUsuarios.ListarAsync();
        var esUrgente = fechaEntrega < DateTime.Now.AddDays(3);

        var cargas = usuarios
            .Where(usuario => !string.IsNullOrWhiteSpace(usuario.Nombre))
            .Select(usuario => usuario.Nombre.Trim())
            .Distinct()
            .Select(nombre =>
            {
                var trabajos = listaTrabajo.Where(x => x.NombreUsuarioAsignado == nombre).ToList();
                var pendientes = trabajos.Where(item => item.Estado == "Pendiente").ToList();

                return new
                {
                    Nombre = nombre,
                    TotalAsignados = trabajos.Count,
                    TotalPendientes = pendientes.Count,
                    PendientesAltaRelevancia = pendientes.Count(item => item.Relevancia == "Alta"),
                };
            });

        string? usuarioDisponible = string.Empty;

        if (esUrgente)
        {
            var trabajadorMasLigero = cargas.OrderBy(x => x.TotalPendientes).FirstOrDefault();
            return trabajadorMasLigero?.Nombre ?? throw new ValidationException("No hay usuarios disponibles para asignar el ítem.");
        }
        else
        {

            if (relevancia == "Alta")
            {
                // Tres ítems altos todavía permiten una asignación; la saturación comienza al superar tres.
                var trabajadorMasLigero = cargas
                    .Where(x => x.PendientesAltaRelevancia <= 3)
                    .OrderBy(x => x.TotalPendientes)
                    .ThenBy(x => x.Nombre)
                    .FirstOrDefault();
                return trabajadorMasLigero?.Nombre ?? throw new ValidationException("Todos los usuarios estan saturados");
            }
            return usuarioActual;
        }
    }

}
