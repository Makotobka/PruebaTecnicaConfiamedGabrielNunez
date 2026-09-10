using System.Data;
using Microsoft.Data.SqlClient;
using Trabajos.Api.Modelos;

namespace Trabajos.Api.Repositorios;

public class RepositorioItemsTrabajo : IRepositorioItemsTrabajo
{
    private readonly string cadenaConexion;

    public RepositorioItemsTrabajo(IConfiguration configuracion)
    {
        cadenaConexion = configuracion.GetConnectionString("BaseDatos")
            ?? throw new InvalidOperationException("Falta configurar la conexión BaseDatos.");
    }

    public async Task<List<ItemTrabajo>> ListarAsync()
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("SELECT IdItem, Titulo, FechaCreacion, FechaEntrega, Relevancia, Estado, NombreUsuarioAsignado, FechaAsignacion, FechaCompletado FROM dbo.ItemsTrabajo ORDER BY IdItem", conexion);
        await using var lector = await comando.ExecuteReaderAsync();
        var resultados = new List<ItemTrabajo>();
        while (await lector.ReadAsync())
            resultados.Add(Leer(lector));
        return resultados;
    }

    public async Task<ItemTrabajo?> ObtenerAsync(Guid id)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("SELECT IdItem, Titulo, FechaCreacion, FechaEntrega, Relevancia, Estado, NombreUsuarioAsignado, FechaAsignacion, FechaCompletado FROM dbo.ItemsTrabajo WHERE IdItem = @Id", conexion);
        comando.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
        await using var lector = await comando.ExecuteReaderAsync();
        return await lector.ReadAsync() ? Leer(lector) : null;
    }

    public async Task<ItemTrabajo> CrearAsync(ItemTrabajo entidad)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("INSERT INTO dbo.ItemsTrabajo (Titulo, FechaCreacion, FechaEntrega, Relevancia, Estado, NombreUsuarioAsignado, FechaAsignacion, FechaCompletado) OUTPUT INSERTED.IdItem VALUES (@Titulo, @FechaCreacion, @FechaEntrega, @Relevancia, @Estado, @NombreUsuarioAsignado, @FechaAsignacion, @FechaCompletado)", conexion);
        AgregarParametros(comando, entidad);
        entidad.IdItem = (Guid)(await comando.ExecuteScalarAsync())!;
        return entidad;
    }

    public async Task<bool> ActualizarAsync(ItemTrabajo entidad)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("UPDATE dbo.ItemsTrabajo SET Titulo = @Titulo, FechaEntrega = @FechaEntrega, Relevancia = @Relevancia, Estado = @Estado, NombreUsuarioAsignado = @NombreUsuarioAsignado, FechaAsignacion = @FechaAsignacion, FechaCompletado = @FechaCompletado WHERE IdItem = @Id", conexion);
        comando.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = entidad.IdItem;
        AgregarParametros(comando, entidad);
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("DELETE FROM dbo.ItemsTrabajo WHERE IdItem = @Id", conexion);
        comando.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    private async Task<SqlConnection> AbrirConexionAsync()
    {
        var conexion = new SqlConnection(cadenaConexion);
        try
        {
            await conexion.OpenAsync();
            return conexion;
        }
        catch
        {
            await conexion.DisposeAsync();
            throw;
        }
    }

    private static void AgregarParametros(SqlCommand comando, ItemTrabajo entidad)
    {
        comando.Parameters.Add("@Titulo", SqlDbType.NVarChar, 200).Value = entidad.Titulo;
        comando.Parameters.Add("@FechaCreacion", SqlDbType.DateTime2).Value = entidad.FechaCreacion;
        comando.Parameters.Add("@FechaEntrega", SqlDbType.DateTime2).Value = entidad.FechaEntrega;
        comando.Parameters.Add("@Relevancia", SqlDbType.NVarChar, 4).Value = entidad.Relevancia;
        comando.Parameters.Add("@Estado", SqlDbType.NVarChar, 10).Value = entidad.Estado;
        comando.Parameters.Add("@NombreUsuarioAsignado", SqlDbType.NVarChar, 100).Value = (object?)entidad.NombreUsuarioAsignado ?? DBNull.Value;
        comando.Parameters.Add("@FechaAsignacion", SqlDbType.DateTime2).Value = (object?)entidad.FechaAsignacion ?? DBNull.Value;
        comando.Parameters.Add("@FechaCompletado", SqlDbType.DateTime2).Value = (object?)entidad.FechaCompletado ?? DBNull.Value;
    }

    private static ItemTrabajo Leer(SqlDataReader lector)
    {
        return new ItemTrabajo
        {
            IdItem = lector.GetGuid(0),
            Titulo = lector.GetString(1),
            FechaCreacion = DateTime.SpecifyKind(lector.GetDateTime(2), DateTimeKind.Utc),
            FechaEntrega = DateTime.SpecifyKind(lector.GetDateTime(3), DateTimeKind.Utc),
            Relevancia = lector.GetString(4),
            Estado = lector.GetString(5),
            NombreUsuarioAsignado = lector.IsDBNull(6) ? null : lector.GetString(6),
            FechaAsignacion = lector.IsDBNull(7) ? null : DateTime.SpecifyKind(lector.GetDateTime(7), DateTimeKind.Utc),
            FechaCompletado = lector.IsDBNull(8) ? null : DateTime.SpecifyKind(lector.GetDateTime(8), DateTimeKind.Utc)
        };
    }
}
