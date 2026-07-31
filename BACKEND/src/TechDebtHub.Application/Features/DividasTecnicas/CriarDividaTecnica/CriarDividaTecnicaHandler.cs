using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Domain.Common;
using TechDebtHub.Domain.Entities;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica
{
    public class CriarDividaTecnicaHandler
    {
        private readonly IApplicationDbContext _context;

        public CriarDividaTecnicaHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CriarDividaTecnicaResponse> HandleAsync(
            CriarDividaTecnicaCommand command,
            CancellationToken cancellationToken
        )
        {
            var projeto = await _context
                .Projetos.AsNoTracking()
                .Where(projeto => projeto.Id == command.ProjetoId)
                .Select(projeto => new { projeto.Id, projeto.Arquivado })
                .FirstOrDefaultAsync();

            if (projeto is null)
            {
                throw new NotFoundException("Projeto não encontrado");
            }

            if (projeto.Arquivado)
            {
                throw new DomainException(
                    "Não é possível criar uma dívida em um projeto arquivado."
                );
            }

            var tituloNormalizado = TextNormalizer.NormalizarParaComparacao(command.Titulo);

            var tituloJaExiste = await _context
                .DividasTecnicas.AsNoTracking()
                .AnyAsync(
                    divida =>
                        divida.ProjetoId == command.ProjetoId
                        && divida.TituloNormalizado == tituloNormalizado,
                    cancellationToken
                );

            if (tituloJaExiste)
            {
                throw new ConflictException("Já existe uma dívida com esse título neste projeto.");
            }

            var divida = DividaTecnica.Criar(
                command.ProjetoId,
                command.Titulo,
                command.Descricao,
                command.Categoria,
                command.Impacto,
                command.Urgencia,
                command.Frequencia,
                command.Esforco
            );

            _context.DividasTecnicas.Add(divida);

            await _context.SaveChangesAsync(cancellationToken);

            return new CriarDividaTecnicaResponse(
                divida.Id,
                divida.ProjetoId,
                divida.Titulo,
                divida.Descricao,
                divida.Categoria,
                divida.Impacto,
                divida.Urgencia,
                divida.Frequencia,
                divida.Esforco,
                divida.PontuacaoPrioridade,
                divida.DataCriacao
            );
        }
    }
}
