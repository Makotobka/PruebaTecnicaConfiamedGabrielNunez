namespace Trabajos.Api.Modelos;

public class ItemTrabajo
{
    public Guid IdItem { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaEntrega { get; set; }
    public string Relevancia { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? NombreUsuarioAsignado { get; set; }
    public DateTime? FechaAsignacion { get; set; }
    public DateTime? FechaCompletado { get; set; }
}
