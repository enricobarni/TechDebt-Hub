using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Application.Features.Projetos.CriarProjeto
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
            var projeto = Projeto.Criar(command.Nome, command.Descricao);

            var nomeJaExiste = await _context
                .Projetos.AsNoTracking()
                .AnyAsync(p => p.NomeNormalizado == projeto.NomeNormalizado, cancellationToken);

            if (nomeJaExiste)
            {
                throw new ConflictException("Já existe um projeto com esse nome");
            }

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
