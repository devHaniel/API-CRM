namespace Front.Models.Auth
{
    public class RegisterViewModel
    {
        public RegisterInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }
}