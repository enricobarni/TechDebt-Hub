using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Projetos.CriarProjeto
{
    public sealed record CriarProjetoResponse(
        Guid Id,
        string Nome,
        string Descricao,
        DateTime DataCriacao
    );
}
