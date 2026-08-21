using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Usuarios.ConfirmarEmail
{
    public sealed record ConfirmarEmailCommand(string Email, string Codigo);
}
