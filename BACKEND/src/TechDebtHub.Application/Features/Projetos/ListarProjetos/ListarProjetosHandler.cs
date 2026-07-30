using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;

namespace TechDebtHub.Application.Features.Projetos.ListarProjetos
{
    public sealed class ListarProjetosHandler
    {
        private readonly IApplicationDbContext _context;

        public ListarProjetosHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ProjetoResumoResponse>> HandlerAsync(
            ListarProjetosQuery query,
            CancellationToken cancellationToken
        )
        {
            return await _context
                .Projetos.AsNoTracking()
                .Where(projeto => !projeto.Arquivado)
                .OrderByDescending(projeto => projeto.DataAtualizacao ?? projeto.DataAtualizacao)
                .Select(projeto => new ProjetoResumoResponse(
                    projeto.Id,
                    projeto.Nome,
                    projeto.Descricao,
                    projeto.DataCriacao,
                    projeto.DataAtualizacao,
                    projeto.Arquivado
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
