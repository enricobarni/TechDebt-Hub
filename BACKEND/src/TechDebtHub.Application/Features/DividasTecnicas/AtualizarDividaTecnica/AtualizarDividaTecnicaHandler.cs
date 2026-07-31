using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Domain.Common;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Application.Features.DividasTecnicas.AtualizarDividaTecnica
{
    public sealed class AtualizarDividaTecnicaHandler
    {
        private readonly IApplicationDbContext _context;

        public AtualizarDividaTecnicaHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AtualizarDividaTecnicaResponse> HandleAsync(
            AtualizarDividaTecnicaCommand command,
            CancellationToken cancellationToken
        )
        {
            var divida = await _context.DividasTecnicas.FirstOrDefaultAsync(
                divida => divida.Id == command.Id,
                cancellationToken
            );

            if (divida is null)
            {
                throw new NotFoundException("Dívida técnica não encontrada");
            }

            var tituloNormalizado = TextNormalizer.NormalizarParaComparacao(command.Titulo);

            var tituloPertenceAOutraDivida = await _context
                .DividasTecnicas.AsNoTracking()
                .AnyAsync(
                    outraDivida =>
                        outraDivida.Id != command.Id
                        && outraDivida.ProjetoId == divida.ProjetoId
                        && outraDivida.TituloNormalizado == tituloNormalizado,
                    cancellationToken
                );

            if (tituloPertenceAOutraDivida)
            {
                throw new ConflictException(
                    "Já existe outra dívida com esse título neste projeto."
                );
            }

            divida.AtualizarDividaTecnica(
                command.Titulo,
                command.Descricao,
                command.Categoria,
                command.Impacto,
                command.Urgencia,
                command.Frequencia,
                command.Esforco
            );

            await _context.SaveChangesAsync(cancellationToken);

            return new AtualizarDividaTecnicaResponse(
                divida.Id,
                divida.ProjetoId,
                divida.Titulo,
                divida.Descricao,
                divida.Categoria,
                divida.Status,
                divida.Impacto,
                divida.Urgencia,
                divida.Frequencia,
                divida.Esforco,
                divida.PontuacaoPrioridade,
                divida.DataAtualizacao
            );
        }
    }
}
