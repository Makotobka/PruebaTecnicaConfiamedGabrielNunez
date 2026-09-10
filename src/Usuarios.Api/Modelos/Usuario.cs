namespace Usuarios.Api.Modelos;

/// <summary>
/// Representa un usuario disponible para recibir ítems de trabajo.
/// </summary>
public class Usuario
{
    public Guid Id { get; set; }
    public string Nick { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
