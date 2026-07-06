using System.ComponentModel.DataAnnotations;

namespace Front.Models.Auth
{
    public class RegisterInputModel
    {
        [Required(ErrorMessage = "El nombre del negocio es obligatorio")]
        [MaxLength(150)]
        [Display(Name = "Nombre del negocio")]
        public string NombreNegocio { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rubro es obligatorio")]
        [MaxLength(80)]
        public string Rubro { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un email válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la contraseña")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del administrador es obligatorio")]
        [Display(Name = "Tu nombre")]
        public string NombreAdmin { get; set; } = string.Empty;
    }
}
