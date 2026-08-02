using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;

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

            divida.Arquivar();

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
