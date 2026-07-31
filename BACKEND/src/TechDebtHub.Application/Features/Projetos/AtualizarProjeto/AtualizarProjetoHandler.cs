using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;

namespace TechDebtHub.Application.Features.Projetos.AtualizarProjeto
{
    public sealed class AtualizarProjetoHandler
    {
        private readonly IApplicationDbContext _context;

        public AtualizarProjetoHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AtualizarProjetoResponse> HandleAsync(
            AtualizarProjetoCommand command,
            CancellationToken cancellationToken
        )
        {
            var projeto = await _context.Projetos.FirstOrDefaultAsync(
                projeto => projeto.Id == command.Id,
                cancellationToken
            );

            if (projeto is null)
            {
                throw new NotFoundException("Projeto Não Encontrado");
            }

            projeto.AtualizarProjeto(command.Nome, command.Descricao);

            await _context.SaveChangesAsync(cancellationToken);

            return new AtualizarProjetoResponse(
                projeto.Id,
                projeto.Nome,
                projeto.Descricao,
                projeto.DataAtualizacao
            );
        }
    }
}
