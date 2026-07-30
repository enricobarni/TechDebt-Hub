using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Api.Contracts.Projetos
{
    public sealed record CriarProjetoRequest(string Nome, string Descricao);
}
