using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Application.Features.CriarProjeto
{
    public sealed class CriarProjetoHandler
    {
        private readonly IApplicationDbContext _context;

        public CriarProjetoHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CriarProjetoResponse> HandleAsync(
            CriarProjetoCommand command,
            CancellationToken cancellationToken
        )
        {
            var projeto = new Projeto(
                command.Nome,
                command.Descricao
            );

            _context.Projetos.Add(projeto);

            await _context.SaveChangesAsync(cancellationToken);

            return new CriarProjetoResponse(
                projeto.Id,
                projeto.Nome,
                projeto.Descricao,
                projeto.DataCriacao
            );
        }
    }
}
