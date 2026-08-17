using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string destinatario, string assunto, string html);
    }
}
