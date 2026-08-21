using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Api.Contracts.Auth
{
    public sealed record ConfirmarEmailRequest(string Email, string Codigo);
}
