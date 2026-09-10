using System.Data;
using Microsoft.Data.SqlClient;
using Usuarios.Api.Modelos;

namespace Usuarios.Api.Repositorios;

/// <summary>
/// Implementa el acceso parametrizado a la tabla de usuarios en SQL Server.
/// </summary>
public class RepositorioUsuarios : IRepositorioUsuarios
{
    private readonly string cadenaConexion;

    /// <summary>Inicializa el repositorio con la cadena de conexión configurada.</summary>
    public RepositorioUsuarios(IConfiguration configuracion)
    {
        cadenaConexion = configuracion.GetConnectionString("BaseDatos")
            ?? throw new InvalidOperationException("Falta configurar la conexión BaseDatos.");
    }

    public async Task<List<Usuario>> ListarAsync()
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("SELECT Id, Nick, Nombre FROM dbo.Usuarios ORDER BY Id", conexion);
        await using var lector = await comando.ExecuteReaderAsync();
        var resultados = new List<Usuario>();
        while (await lector.ReadAsync())
            resultados.Add(Leer(lector));
        return resultados;
    }

    public async Task<Usuario?> ObtenerAsync(Guid id)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("SELECT Id, Nick, Nombre FROM dbo.Usuarios WHERE Id = @Id", conexion);
        comando.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
        await using var lector = await comando.ExecuteReaderAsync();
        return await lector.ReadAsync() ? Leer(lector) : null;
    }

    public async Task<Usuario> CrearAsync(Usuario entidad)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("INSERT INTO dbo.Usuarios (Nick, Nombre) OUTPUT INSERTED.Id VALUES (@Nick, @Nombre)", conexion);
        AgregarParametros(comando, entidad);
        entidad.Id = (Guid)(await comando.ExecuteScalarAsync())!;
        return entidad;
    }

    public async Task<bool> ActualizarAsync(Usuario entidad)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("UPDATE dbo.Usuarios SET Nick = @Nick, Nombre = @Nombre WHERE Id = @Id", conexion);
        comando.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = entidad.Id;
        AgregarParametros(comando, entidad);
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> EliminarAsync(Guid id)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("DELETE FROM dbo.Usuarios WHERE Id = @Id", conexion);
        comando.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
        return await comando.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> ExisteNickAsync(string nick)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("SELECT TOP (1) 1 FROM dbo.Usuarios WHERE Nick = @Nick", conexion);
        comando.Parameters.Add("@Nick", SqlDbType.NVarChar, 100).Value = nick;
        return await comando.ExecuteScalarAsync() is not null;
    }

    public async Task<bool> ExisteUserAsync(string nombre)
    {
        await using var conexion = await AbrirConexionAsync();
        await using var comando = new SqlCommand("SELECT TOP (1) 1 FROM dbo.Usuarios WHERE Nombre = @nombre", conexion);
        comando.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = nombre;
        return await comando.ExecuteScalarAsync() is not null;
    }

    /// <summary>Abre una conexión nueva para la operación en curso.</summary>
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

    /// <summary>Agrega al comando los parámetros editables del usuario.</summary>
    private static void AgregarParametros(SqlCommand comando, Usuario entidad)
    {
        comando.Parameters.Add("@Nick", SqlDbType.NVarChar, 100).Value = entidad.Nick;
        comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, 200).Value = entidad.Nombre;
    }

    /// <summary>Convierte la fila actual del lector en un usuario.</summary>
    private static Usuario Leer(SqlDataReader lector)
    {
        return new Usuario
        {
            Id = lector.GetGuid(0),
            Nick = lector.GetString(1),
            Nombre = lector.GetString(2)
        };
    }
}
