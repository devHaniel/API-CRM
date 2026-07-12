using System.ComponentModel.DataAnnotations;

namespace Front.Models.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un email válido")]
        public string Email { get; set; } = string.Empty;

        public bool Enviado { get; set; }
    }
}
