using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;

namespace TechDebtHub.Application.Features.Projetos.BuscarPorId
{
    public class BuscarPorIdHandler
    {
        private readonly IApplicationDbContext _context;

        public BuscarPorIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BuscarPorIdResponse> HandlerAsync(
            BuscarPorIdQuery query,
            CancellationToken cancellationToken
        )
        {
            var projeto = await _context
                .Projetos.AsNoTracking()
                .Where(projeto => projeto.Id == query.Id)
                .Select(projeto => new BuscarPorIdResponse(
                    projeto.Id,
                    projeto.Nome,
                    projeto.Descricao,
                    projeto.DataCriacao,
                    projeto.DataAtualizacao,
                    projeto.Arquivado
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (projeto is null)
            {
                throw new NotFoundException("Projeto não encontrado");
            }

            return projeto;
        }
    }
}
