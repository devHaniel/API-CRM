using System.ComponentModel.DataAnnotations;

namespace Application.Configuration
{
    public class StripeSettings
    {
        [Required(ErrorMessage = "Stripe:SecretKey es obligatorio.")]
        public string SecretKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Stripe:PublishableKey es obligatorio.")]
        public string PublishableKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Stripe:WebhookSecret es obligatorio.")]
        public string WebhookSecret { get; set; } = string.Empty;
    }
}
