using System.ComponentModel.DataAnnotations;

/// <summary>
/// Contiene los datos editables enviados para crear o actualizar un ítem de trabajo.
/// </summary>
public class SolicitudItemTrabajo
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título admite hasta 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de entrega es obligatoria.")]
    public DateTime? FechaEntrega { get; set; }

    [Required(ErrorMessage = "La relevancia es obligatoria.")]
    [RegularExpression("^(Alta|Baja)$", ErrorMessage = "La relevancia debe ser Alta o Baja.")]
    public string Relevancia { get; set; } = string.Empty;

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(Pendiente|Completado)$", ErrorMessage = "El estado debe ser Pendiente o Completado.")]
    public string Estado { get; set; } = "Pendiente";

    [StringLength(100, ErrorMessage = "El usuario asignado admite hasta 100 caracteres.")]
    public string? NombreUsuarioAsignado { get; set; }
    public string? NickUsuarioAsignado { get; set; }
}
