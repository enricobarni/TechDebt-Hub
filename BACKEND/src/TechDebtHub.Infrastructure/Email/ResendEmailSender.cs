using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Resend;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Infrastructure.Email
{
    public sealed class ResendEmailSender : IEmailSender
    {
        private readonly IResend _resend;
        private readonly EmailSettings _settings;

        public ResendEmailSender(IResend resend, IOptions<EmailSettings> options)
        {
            _resend = resend;
            _settings = options.Value;
        }

        public async Task SendAsync(
            string destinatario,
            string assunto,
            string html,
            CancellationToken cancellationToken = default
        )
        {
            var message = new EmailMessage
            {
                From = $"{_settings.FromName} <{_settings.FromEmail}>",
                Subject = assunto,
                HtmlBody = html,
            };

            message.To.Add(destinatario);

            var response = await _resend.EmailSendAsync(message, cancellationToken);

            if (!response.Success)
            {
                throw new InvalidOperationException(
                    "Não foi possível enviar o e-mail",
                    response.Exception
                );
            }
        }
    }
}
