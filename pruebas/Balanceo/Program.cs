using System.ComponentModel.DataAnnotations;
using Trabajos.Api.Modelos;
using Trabajos.Api.Repositorios;
using Trabajos.Api.Servicios;

var ahora = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc);
var pruebas = 0;

List<ItemTrabajo> Trabajos(string usuario, int pendientes, int altas = 0, int completados = 0)
{
    return Enumerable.Range(0, pendientes + completados).Select(indice => new ItemTrabajo
    {
        NombreUsuarioAsignado = usuario,
        Estado = indice < pendientes ? "Pendiente" : "Completado",
        Relevancia = indice < altas || indice >= pendientes ? "Alta" : "Baja"
    }).ToList();
}

async Task Verificar(string caso, List<ItemTrabajo> trabajos, string[] usuarios, DateTime fecha, string actual, string? esperado)
{
    var repositorio = new RepositorioPrueba(trabajos);
    var servicio = new ServicioItemsTrabajo(repositorio, new UsuariosPrueba(usuarios));
    try
    {
        var resultado = await servicio.CrearAsync(new SolicitudItemTrabajo
        {
            Titulo = "Prueba de reparto", FechaEntrega = fecha, Relevancia = "Alta",
            Estado = "Pendiente", NombreUsuarioAsignado = actual
        });
        if (esperado is null || resultado.NombreUsuarioAsignado != esperado)
            throw new Exception($"{caso}: se esperaba {esperado ?? "rechazo"}, se obtuvo {resultado.NombreUsuarioAsignado}.");
    }
    catch (ValidationException error) when (esperado is null && (error.Message.Contains("No hay usuarios disponibles") || error.Message.Contains("Todos los usuarios estan saturados")))
    {
        if (repositorio.Creaciones != 0) throw new Exception("Se guardó un ítem sin usuario elegible.");
    }
    Console.WriteLine($"Correcto: {caso}");
    pruebas++;
}

// Ejecutar solo los casos del criterio de carga para relevancia alta no urgente.
if (args.Contains("--altas-no-urgentes"))
{
    var fechaNoUrgente = DateTime.Now.AddDays(10);
    await Verificar("Menos pendientes totales aunque tenga mas altas", [..Trabajos("A", 8), ..Trabajos("B", 2, 2)], ["A", "B"], fechaNoUrgente, "A", "B");
    await Verificar("Excluye saturado aunque tenga menos pendientes", [..Trabajos("A", 4, 4), ..Trabajos("B", 5, 1)], ["A", "B"], fechaNoUrgente, "A", "B");
    await Verificar("Exactamente tres altas sigue siendo elegible", [..Trabajos("A", 3, 3), ..Trabajos("B", 4)], ["A", "B"], fechaNoUrgente, "B", "A");
    await Verificar("Ignora completados al medir carga", [..Trabajos("A", 1, 1, 10), ..Trabajos("B", 2)], ["A", "B"], fechaNoUrgente, "B", "A");
    await Verificar("Incluye usuario sin pendientes", Trabajos("A", 1), ["A", "B"], fechaNoUrgente, "A", "B");
    await Verificar("Todos saturados no guarda", [..Trabajos("A", 4, 4), ..Trabajos("B", 4, 4)], ["A", "B"], fechaNoUrgente, "A", null);
    Console.WriteLine($"{pruebas} escenarios de relevancia alta no urgente verificados.");
    return;
}

await Verificar("Ejemplo A/B devuelve B", [..Trabajos("A", 3, 2), ..Trabajos("B", 1)], ["A", "B"], ahora.AddDays(2), "A", "B");
await Verificar("Incluye usuarios sin ítems", Trabajos("A", 1), ["A", "B"], ahora.AddDays(5), "A", "B");
await Verificar("Excluye saturado aunque sea urgente", [..Trabajos("A", 4, 4), ..Trabajos("B", 5)], ["A", "B"], ahora.AddDays(1), "A", "B");
await Verificar("Tres altas todavía permiten asignar", [..Trabajos("A", 3, 3), ..Trabajos("B", 4)], ["A", "B"], ahora.AddDays(5), "B", "A");
await Verificar("Completados no saturan ni cuentan como pendientes", [..Trabajos("A", 0, 0, 8), ..Trabajos("B", 1)], ["A", "B"], ahora.AddDays(5), "B", "A");
await Verificar("Urgentes usan total asignado", [..Trabajos("A", 0, 0, 8), ..Trabajos("B", 1)], ["A", "B"], ahora.AddDays(2), "A", "B");
await Verificar("Exactamente 72 horas no es urgente", [..Trabajos("A", 0, 0, 8), ..Trabajos("B", 1)], ["A", "B"], ahora.AddDays(3), "B", "A");
await Verificar("Vencidos son urgentes", [..Trabajos("A", 0, 0, 8), ..Trabajos("B", 1)], ["A", "B"], ahora.AddDays(-1), "A", "B");
await Verificar("Empate conserva usuario actual", [], ["A", "B"], ahora.AddDays(2), "B", "B");
await Verificar("Empate restante por nombre", [], ["C", "B"], ahora.AddDays(2), "A", "B");
await Verificar("Normaliza mayúsculas y espacios", Trabajos(" a ", 2), ["A", "B"], ahora.AddDays(2), "A", "B");
await Verificar("No asigna a usuarios eliminados", Trabajos("A", 1), ["A"], ahora.AddDays(2), "B", "A");
await Verificar("Todos saturados rechaza sin guardar", [..Trabajos("A", 4, 4), ..Trabajos("B", 4, 4)], ["A", "B"], ahora.AddDays(2), "A", null);
await Verificar("Sin usuarios rechaza sin guardar", [], [], ahora.AddDays(2), "A", null);
Console.WriteLine($"{pruebas} escenarios de balanceo verificados.");

class UsuariosPrueba(string[] nombres) : IConsultaUsuarios
{
    public Task<List<UsuarioReferencia>> ListarAsync() => Task.FromResult(nombres.Select(nombre => new UsuarioReferencia { Nombre = nombre }).ToList());
    public Task<bool> ExisteNickAsync(string nick) => Task.FromResult(true);
    public Task<bool> ExisteNombreAsync(string nombre) => Task.FromResult(true);
}

class RepositorioPrueba(List<ItemTrabajo> trabajos) : IRepositorioItemsTrabajo
{
    public int Creaciones { get; private set; }
    public Task<List<ItemTrabajo>> ListarAsync() => Task.FromResult(trabajos);
    public Task<ItemTrabajo> CrearAsync(ItemTrabajo entidad) { Creaciones++; return Task.FromResult(entidad); }
    public Task<ItemTrabajo?> ObtenerAsync(Guid id) => throw new NotSupportedException();
    public Task<bool> ActualizarAsync(ItemTrabajo entidad) => throw new NotSupportedException();
    public Task<bool> EliminarAsync(Guid id) => throw new NotSupportedException();
}
