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
                <div style=""background-color:#26215C; padding:32px 40px; text-align:center; border-radius:12px 12px 0 0;"">
                    <div style=""font-size:22px; font-weight:bold; color:#FFFFFF;"">RecordApp</div>
                </div>
                <div style=""background:#fff; padding:32px 40px; font-family:Arial, sans-serif; color:#444441;"">
                    <h1 style=""color:#2C2C2A; font-size:22px; margin-top:0;"">¡Bienvenido a RecordApp!</h1>
                    <p>Hola equipo de <strong>{nombre}</strong>,</p>
                    <p>Tu cuenta de empresa ha sido creada correctamente. Ya puedes iniciar sesión y comenzar a utilizar la plataforma.</p>
                    <p style=""color:#888780; font-size:13px;"">Gracias por confiar en nosotros.<br><strong>Equipo RecordApp</strong></p>
                </div>";

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
                <div style=""background-color:#26215C; padding:32px 40px; text-align:center; border-radius:12px 12px 0 0;"">
                    <div style=""font-size:22px; font-weight:bold; color:#FFFFFF;"">RecordApp</div>
                </div>
                <div style=""background:#fff; padding:32px 40px; font-family:Arial, sans-serif; color:#444441;"">
                    <h2 style=""color:#2C2C2A; font-size:20px; margin-top:0;"">Recuperación de contraseña</h2>

                    <p>Hola <strong>{nombre}</strong>,</p>

                    <p>Recibimos una solicitud para restablecer tu contraseña.</p>

                    <p style=""font-size:14px; color:#555;"">Ingresa el siguiente código de verificación:</p>

                    <div style=""background:#f4f4f6; padding:16px; border-radius:8px; text-align:center; font-size:32px; font-weight:bold; letter-spacing:8px; color:#26215C; margin:20px 0;"">
                        {resetLink}
                    </div>

                    <p style=""font-size:13px; color:#888;"">Este código expira en 15 minutos.</p>

                    <p style=""color:#888780; font-size:13px;"">Si tú no realizaste esta solicitud, puedes ignorar este correo.</p>

                    <p style=""color:#888780; font-size:13px;"">Gracias,<br><strong>Equipo RecordApp</strong></p>
                </div>";

            return await SendEmailAsync(
                to,
                "Recuperación de contraseña - Código de verificación",
                html,
                ct);
        }
    }
}