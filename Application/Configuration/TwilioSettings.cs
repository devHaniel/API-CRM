using System.ComponentModel.DataAnnotations;

namespace Application.Configuration
{
    public class TwilioSettings
    {
        [Required(ErrorMessage = "El Twilio:AccountSid es obligatorio.")]
        public string AccountSid { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Twilio:AuthToken es obligatorio.")]
        public string AuthToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "El Twilio:FromNumber es obligatorio.")]
        public string FromNumber { get; set; } = string.Empty;
    }
}
