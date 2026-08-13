using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Common.Models;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica;
using TechDebtHub.Domain.Common;
using TechDebtHub.Domain.Entities;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.ListarDividasTecnicas
{
    public sealed class ListarDividasTecnicasHandler
    {
        private readonly IApplicationDbContext _context;

        public ListarDividasTecnicasHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ListarDividasTecnicasResponse>> HandleAsync(
            ListarDividasTecnicasQuery query,
            CancellationToken cancellationToken
        )
        {
            int maximoPaginas = 100;

            if (query.Pagina < 1)
            {
                throw new ArgumentException("A página deve ser maior ou igual a 1");
            }
            if (query.TamanhoPagina < 1 || query.TamanhoPagina > maximoPaginas)
            {
                throw new ArgumentException("O tamanho da página deve estar entre 1 e 100");
            }
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

            if (query.Arquivada.HasValue)
            {
                consulta = consulta.Where(divida => divida.Arquivada == query.Arquivada.Value);
            }
            else
            {
                consulta = consulta.Where(divida => !divida.Arquivada);
            }

            if (query.Status.HasValue)
            {
                consulta = consulta.Where(divida => divida.Status == query.Status.Value);
            }
            else
            {
                consulta = consulta.Where(divida => divida.Status != StatusDivida.Resolvida);
            }

            if (query.Categoria.HasValue)
            {
                consulta = consulta.Where(divida => divida.Categoria == query.Categoria.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Busca))
            {
                var busca = TextNormalizer.NormalizarParaComparacao(query.Busca);

                consulta = consulta.Where(divida => divida.TituloNormalizado.Contains(busca));
            }

            var totalItens = await consulta.CountAsync(cancellationToken);

            var totalPaginas = (int)Math.Ceiling(totalItens / (double)query.TamanhoPagina);

            var itens = await consulta
                .OrderByDescending(divida => divida.PontuacaoPrioridade)
                .ThenByDescending(divida => divida.DataCriacao)
                .ThenBy(divida => divida.Id)
                .Skip((query.Pagina - 1) * query.TamanhoPagina)
                .Take(query.TamanhoPagina)
                .Select(divida => new ListarDividasTecnicasResponse(
                    divida.Id,
                    divida.Titulo,
                    divida.Categoria,
                    divida.Status,
                    divida.Arquivada,
                    divida.Impacto,
                    divida.Urgencia,
                    divida.Frequencia,
                    divida.Esforco,
                    divida.PontuacaoPrioridade,
                    divida.DataCriacao,
                    divida.DataAtualizacao
                ))
                .ToListAsync(cancellationToken);

            return new PagedResult<ListarDividasTecnicasResponse>(
                itens,
                query.Pagina,
                query.TamanhoPagina,
                totalItens,
                totalPaginas
            );
        }
    }
}
