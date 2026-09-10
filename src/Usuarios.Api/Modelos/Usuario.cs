namespace Usuarios.Api.Modelos;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nick { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}
