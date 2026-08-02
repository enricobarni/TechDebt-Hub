using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.DividasTecnicas.BuscarDividaTecnicaPorId;

namespace TechDebtHub.Application.Features.DividasTecnicas.BuscarDividaTecnicaPorId
{
    public sealed class BuscarDividaTecnicaPorIdHandler
    {
        private readonly IApplicationDbContext _context;

        public BuscarDividaTecnicaPorIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BuscarDividaTecnicaPorIdResponse> HandleAsync(
            BuscarDividaTecnicaPorIdQuery query,
            CancellationToken cancellationToken
        )
        {
            var divida = await _context
                .DividasTecnicas.AsNoTracking()
                .Where(divida => divida.Id == query.Id)
                .Select(divida => new BuscarDividaTecnicaPorIdResponse(
                    divida.Id,
                    divida.ProjetoId,
                    divida.Titulo,
                    divida.Descricao,
                    divida.Categoria,
                    divida.Status,
                    divida.Arquivada,
                    divida.Impacto,
                    divida.Urgencia,
                    divida.Frequencia,
                    divida.Esforco,
                    divida.PontuacaoPrioridade,
                    divida.DataCriacao,
                    divida.DataAtualizacao,
                    divida.DataResolucao
                ))
                .FirstOrDefaultAsync();

            if (divida is null)
            {
                throw new NotFoundException("Dívida técnica não encontrada");
            }

            return divida;
        }
    }
}
