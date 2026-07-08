using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken ct = default);

        Task<bool> SendWelcomeEmailAsync(
            string to,
            string nombre,
            CancellationToken ct = default);

        Task<bool> SendPasswordResetEmailAsync(
            string to,
            string nombre,
            string resetLink,
            CancellationToken ct = default);
    }
}