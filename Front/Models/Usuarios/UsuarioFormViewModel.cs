using System.ComponentModel.DataAnnotations;

namespace Front.Models.Usuarios
{
    public class UsuarioFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un email válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } = "Admin";

        public bool Activo { get; set; } = true;
    }
}
