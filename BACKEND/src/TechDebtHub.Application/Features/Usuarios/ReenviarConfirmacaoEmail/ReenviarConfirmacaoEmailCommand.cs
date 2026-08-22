using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Usuarios.ReenviarConfirmacaoEmail
{
    public sealed record ReenviarConfirmacaoEmailCommand(string Email);
}
