using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;

namespace TechDebtHub.Application.Features.Projetos.ArquivarProjeto
{
    public sealed class ArquivarProjetoHandler
    {
        private readonly IApplicationDbContext _context;

        public ArquivarProjetoHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(
            ArquivarProjetoCommand command,
            CancellationToken cancellationToken
        )
        {
            var projeto = await _context.Projetos.FirstOrDefaultAsync(
                projeto => projeto.Id == command.Id,
                cancellationToken
            );

            if (projeto is null)
            {
                throw new NotFoundException("Projeto não encontrado");
            }

            projeto.Arquivar();

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
