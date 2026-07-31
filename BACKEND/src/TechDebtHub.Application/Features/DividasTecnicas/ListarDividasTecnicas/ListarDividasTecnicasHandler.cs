using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Application.Features.DividasTecnicas.ListarDividasTecnicas
{
    public sealed class ListarDividasTecnicasHandler
    {
        private readonly IApplicationDbContext _context;

        public ListarDividasTecnicasHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<ListarDividasTecnicasResponse>> HandleAsync(
            ListarDividasTecnicasQuery query,
            CancellationToken cancellationToken
        )
        {
            if (query.Status.HasValue && !Enum.IsDefined(query.Status.Value))
            {
                throw new ArgumentException("O status é inválido");
            }
            if (query.Categoria.HasValue && !Enum.IsDefined(query.Categoria.Value))
            {
                throw new ArgumentException("A categoria informada é inválida");
            }

            var projetoExiste = await _context
                .Projetos.AsNoTracking()
                .AnyAsync(projeto => projeto.Id == query.ProjetoId, cancellationToken);

            if (!projetoExiste)
            {
                throw new NotFoundException("Projeto não encontrado");
            }

            var consulta = _context
                .DividasTecnicas.AsNoTracking()
                .Where(divida => divida.ProjetoId == query.ProjetoId);

            if (query.Status.HasValue)
            {
                consulta = consulta.Where(divida => divida.Status == query.Status.Value);
            }
            if (query.Categoria.HasValue)
            {
                consulta = consulta.Where(divida => divida.Categoria == query.Categoria.Value);
            }
            if (!string.IsNullOrWhiteSpace(query.Busca))
            {
                var busca = query.Busca.Trim();

                consulta = consulta.Where(divida => divida.Titulo.Contains(busca));
            }

            return await consulta
                .OrderByDescending(divida => divida.PontuacaoPrioridade)
                .ThenByDescending(divida => divida.DataCriacao)
                .ThenBy(divida => divida.Id)
                .Select(divida => new ListarDividasTecnicasResponse(
                    divida.Id,
                    divida.Titulo,
                    divida.Categoria,
                    divida.Status,
                    divida.Impacto,
                    divida.Urgencia,
                    divida.Frequencia,
                    divida.Esforco,
                    divida.PontuacaoPrioridade,
                    divida.DataCriacao,
                    divida.DataAtualizacao
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
