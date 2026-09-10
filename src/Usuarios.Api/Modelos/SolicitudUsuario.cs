using System.ComponentModel.DataAnnotations;

namespace Usuarios.Api.Modelos;

/// <summary>
/// Contiene los datos editables enviados para crear o actualizar un usuario.
/// </summary>
public class SolicitudUsuario
{
    [Required(ErrorMessage = "El nick es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nick admite hasta 100 caracteres.")]
    public string Nick { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(200, ErrorMessage = "El nombre admite hasta 200 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}
