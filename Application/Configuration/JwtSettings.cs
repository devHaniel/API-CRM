using System.ComponentModel.DataAnnotations;

namespace Application.Configuration
{
    public class JwtSettings
    {
        [Required(ErrorMessage = "La clave Jwt:Key es obligatoria.")]
        [MinLength(16, ErrorMessage = "La clave Jwt:Key debe tener al menos 16 caracteres.")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "El emisor Jwt:Issuer es obligatorio.")]
        public string Issuer { get; set; } = string.Empty;

        [Required(ErrorMessage = "La audiencia Jwt:Audience es obligatoria.")]
        public string Audience { get; set; } = string.Empty;

        [Required(ErrorMessage = "La expiración Jwt:ExpiraEnHoras es obligatoria.")]
        public string ExpiraEnHoras { get; set; } = string.Empty;
    }
}
