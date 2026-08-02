using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;

namespace TechDebtHub.Application.Features.DividasTecnicas.AlterarStatusDividaTecnica
{
    public sealed class AlterarStatusDividaTecnicaHandler
    {
        private readonly IApplicationDbContext _context;

        public AlterarStatusDividaTecnicaHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AlterarStatusDividaTecnicaResponse> HandleAsync(
            AlterarStatusDividaTecnicaCommand command,
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

            divida.AlterarStatus(command.NovoStatus);

            await _context.SaveChangesAsync(cancellationToken);

            return new AlterarStatusDividaTecnicaResponse(
                divida.Id,
                divida.Status,
                divida.DataAtualizacao,
                divida.DataResolucao
            );
        }
    }
}
