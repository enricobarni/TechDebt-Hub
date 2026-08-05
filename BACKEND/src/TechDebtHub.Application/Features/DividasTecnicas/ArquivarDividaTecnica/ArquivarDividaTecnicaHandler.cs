using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Domain.Enums;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Application.Features.DividasTecnicas.ArquivarDividaTecnica
{
    public sealed class ArquivarDividaTecnicaHandler
    {
        private readonly IApplicationDbContext _context;

        public ArquivarDividaTecnicaHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(
            ArquivarDividaTecnicaCommand command,
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

            var possuiDividasAtivas = await _context
                .DividasTecnicas.AsNoTracking()
                .AnyAsync(
                    divida =>
                        divida.ProjetoId == command.Id
                        && !divida.Arquivada
                        && divida.Status != StatusDivida.Resolvida,
                    cancellationToken
                );

            if (possuiDividasAtivas)
            {
                throw new DomainException(
                    "Não é possível arquivar um projeto que possui dívidas técnicas ativas"
                );
            }

            divida.Arquivar();

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
