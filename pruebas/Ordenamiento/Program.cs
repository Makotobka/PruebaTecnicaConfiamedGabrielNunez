using System.ComponentModel.DataAnnotations;
using Trabajos.Api.Modelos;
using Trabajos.Api.Repositorios;
using Trabajos.Api.Servicios;

var fecha = DateTime.Now.AddDays(10);
ItemTrabajo Item(int numero, string usuario, string relevancia, int dias, string estado = "Pendiente") => new()
{
    IdItem = Guid.Parse($"00000000-0000-0000-0000-{numero:000000000000}"),
    Titulo = numero.ToString(), NombreUsuarioAsignado = usuario, Relevancia = relevancia,
    FechaEntrega = fecha.AddDays(dias), Estado = estado
};
var datos = new List<ItemTrabajo>
{
    Item(1, "Ana", "Baja", -1), Item(3, "Ana", "Alta", 2),
    Item(5, "Beto", "Alta", 0), Item(4, "Ana", "Alta", -3, "Completado"),
    Item(2, "Ana", "Alta", 1), Item(6, "Ana", "Alta", 1)
};
var repositorio = new RepositorioPrueba(datos);
var servicio = new ServicioItemsTrabajo(repositorio, new UsuariosPrueba());
void Comprobar(bool condicion, string mensaje)
{
    if (!condicion) throw new Exception(mensaje);
    Console.WriteLine($"Correcto: {mensaje}");
}

var pendientes = await servicio.ListarPendientesAsync("Ana");
Comprobar(pendientes.Select(x => x.Titulo).SequenceEqual(new[] { "2", "6", "3", "1" }),
    "Pendientes por usuario: relevancia, fecha y desempate por identificador; excluye completados y otros usuarios.");
var todos = await servicio.ListarAsync();
Comprobar(todos.Select(x => x.Titulo).SequenceEqual(new[] { "2", "6", "3", "1", "4", "5" }),
    "Listado general agrupa por usuario y conserva completados despues de pendientes.");
Comprobar((await servicio.ListarPendientesAsync("Sin trabajos")).Count == 0, "Usuario sin pendientes devuelve lista vacia.");
try
{
    await servicio.ListarPendientesAsync(" ");
    throw new Exception("Se acepto un usuario vacio.");
}
catch (ValidationException) { Console.WriteLine("Correcto: rechaza nombre vacio."); }

var creado = await servicio.CrearAsync(new SolicitudItemTrabajo
{
    Titulo = "Nuevo", NombreUsuarioAsignado = "Ana", Estado = "Pendiente",
    Relevancia = "Baja", FechaEntrega = fecha.AddDays(-2)
});
pendientes = await servicio.ListarPendientesAsync("Ana");
Comprobar(pendientes.Select(x => x.Titulo).SequenceEqual(new[] { "2", "6", "3", "Nuevo", "1" }),
    "Despues de crear y asignar, el nuevo pendiente ocupa su lugar por relevancia y fecha.");
await servicio.ActualizarAsync(creado.IdItem, new SolicitudItemTrabajo
{
    Titulo = "Reasignado", NombreUsuarioAsignado = "Beto", Estado = "Pendiente",
    Relevancia = "Baja", FechaEntrega = fecha.AddDays(-2)
});
Comprobar((await servicio.ListarPendientesAsync("Ana")).All(x => x.IdItem != creado.IdItem),
    "La reasignacion elimina el item de la lista anterior.");
Comprobar((await servicio.ListarPendientesAsync("Beto")).Select(x => x.Titulo).SequenceEqual(new[] { "5", "Reasignado" }),
    "La reasignacion mantiene ordenada la lista del nuevo usuario.");
await servicio.ActualizarAsync(creado.IdItem, new SolicitudItemTrabajo
{
    Titulo = "Prioridad cambiada", NombreUsuarioAsignado = "Beto", Estado = "Pendiente",
    Relevancia = "Alta", FechaEntrega = fecha.AddDays(-2)
});
Comprobar((await servicio.ListarPendientesAsync("Beto")).First().Titulo == "Prioridad cambiada",
    "Cambiar relevancia y fecha reordena el pendiente.");
Console.WriteLine("Todas las pruebas de ordenamiento pasaron sin modificar SQL Server.");

class UsuariosPrueba : IConsultaUsuarios
{
    public Task<List<UsuarioReferencia>> ListarAsync() => Task.FromResult(new List<UsuarioReferencia> { new() { Nombre = "Beto" } });
    public Task<bool> ExisteNickAsync(string nick) => Task.FromResult(true);
    public Task<bool> ExisteNombreAsync(string nombre) => Task.FromResult(true);
}
class RepositorioPrueba(List<ItemTrabajo> datos) : IRepositorioItemsTrabajo
{
    public Task<List<ItemTrabajo>> ListarAsync() => Task.FromResult(datos.ToList());
    public Task<ItemTrabajo?> ObtenerAsync(Guid id) => Task.FromResult(datos.FirstOrDefault(x => x.IdItem == id));
    public Task<ItemTrabajo> CrearAsync(ItemTrabajo entidad)
    {
        entidad.IdItem = Guid.NewGuid(); datos.Add(entidad); return Task.FromResult(entidad);
    }
    public Task<bool> ActualizarAsync(ItemTrabajo entidad) => Task.FromResult(datos.Any(x => x.IdItem == entidad.IdItem));
    public Task<bool> EliminarAsync(Guid id) => Task.FromResult(datos.RemoveAll(x => x.IdItem == id) > 0);
}
