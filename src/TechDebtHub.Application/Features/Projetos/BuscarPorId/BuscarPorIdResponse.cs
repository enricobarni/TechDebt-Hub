using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Features.Projetos.BuscarPorId
{
    public sealed record BuscarPorIdResponse(
        Guid Id,
        string Nome,
        string Descricao,
        DateTime DataCriacao,
        DateTime? DataAtualizacao,
        bool Arquivado
    );
}
