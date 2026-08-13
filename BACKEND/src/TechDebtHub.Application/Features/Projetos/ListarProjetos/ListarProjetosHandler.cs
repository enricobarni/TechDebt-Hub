using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Common.Models;
using TechDebtHub.Domain.Common;

namespace TechDebtHub.Application.Features.Projetos.ListarProjetos
{
    public sealed class ListarProjetosHandler
    {
        private readonly IApplicationDbContext _context;

        public ListarProjetosHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProjetoResumoResponse>> HandleAsync(
            ListarProjetosQuery query,
            CancellationToken cancellationToken
        )
        {
            int maximoPagina = 100;

            if (query.Pagina < 1)
            {
                throw new ArgumentException("Apágina deve ser maior ou igual a 1");
            }
            if (query.TamanhoPagina < 1 || query.TamanhoPagina > maximoPagina)
            {
                throw new ArgumentException("O tamanho da página deve estar entre 1 e 100");
            }

            var consulta = _context.Projetos.AsNoTracking().AsQueryable();

            if (query.Arquivado.HasValue)
            {
                consulta = consulta.Where(projeto => projeto.Arquivado == query.Arquivado.Value);
            }
            else
            {
                consulta = consulta.Where(projeto => !projeto.Arquivado);
            }

            if (!string.IsNullOrWhiteSpace(query.Busca))
            {
                var busca = TextNormalizer.NormalizarParaComparacao(query.Busca);

                consulta = consulta.Where(projeto => projeto.NomeNormalizado.Contains(busca));
            }

            var totalItens = await consulta.CountAsync(cancellationToken);

            var totalPaginas = (int)Math.Ceiling(totalItens / (double)query.TamanhoPagina);

            var itens = await consulta
                .OrderByDescending(projeto => projeto.DataAtualizacao ?? projeto.DataCriacao)
                .ThenBy(projeto => projeto.Id)
                .Skip((query.Pagina - 1) * query.TamanhoPagina)
                .Take(query.TamanhoPagina)
                .Select(projeto => new ProjetoResumoResponse(
                    projeto.Id,
                    projeto.Nome,
                    projeto.Descricao,
                    projeto.DataCriacao,
                    projeto.DataAtualizacao,
                    projeto.Arquivado
                ))
                .ToListAsync(cancellationToken);

            return new PagedResult<ProjetoResumoResponse>(
                itens,
                query.Pagina,
                query.TamanhoPagina,
                totalItens,
                totalPaginas
            );
        }
    }
}
