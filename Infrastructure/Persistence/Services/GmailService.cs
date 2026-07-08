using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces;
using Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Persistence.Services
{
    public class GmailService : IEmailService
    {
        private readonly GmailSettings _settings;

        public GmailService(IOptions<GmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<bool> SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken ct = default)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _settings.DisplayName,
                _settings.Email));

            message.To.Add(MailboxAddress.Parse(to));

            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = htmlBody
            }.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _settings.Host,
                _settings.Port,
                SecureSocketOptions.StartTls,
                ct);

            await smtp.AuthenticateAsync(
                _settings.Email,
                _settings.Password,
                ct);

            await smtp.SendAsync(message, ct);

            await smtp.DisconnectAsync(true, ct);

            return true;
        }

        public async Task<bool> SendWelcomeEmailAsync(
            string to,
            string nombre,
            CancellationToken ct = default)
        {
            var html = $@"
                <h2>¡Bienvenido a RecordApp!</h2>

                <p>Hola <strong>{nombre}</strong>,</p>

                <p>Tu cuenta ha sido creada correctamente.</p>

                <p>Ya puedes iniciar sesión y comenzar a utilizar la plataforma.</p>

                <br>

                <p>Gracias por confiar en nosotros.</p>

                <p><strong>Equipo RecordApp</strong></p>";

            return await SendEmailAsync(
                to,
                "¡Bienvenido a RecordApp!",
                html,
                ct);
        }

        public async Task<bool> SendPasswordResetEmailAsync(
            string to,
            string nombre,
            string resetLink,
            CancellationToken ct = default)
        {
            var html = $@"
                <h2>Recuperación de contraseña</h2>

                <p>Hola <strong>{nombre}</strong>,</p>

                <p>Recibimos una solicitud para restablecer tu contraseña.</p>

                <p>
                    <a href='{resetLink}'>
                        Restablecer contraseña
                    </a>
                </p>

                <p>Si tú no realizaste esta solicitud, puedes ignorar este correo.</p>

                <p>Equipo RecordApp</p>";

            return await SendEmailAsync(
                to,
                "Recuperación de contraseña",
                html,
                ct);
        }
    }
}