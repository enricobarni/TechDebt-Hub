using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.DividasTecnicas;
using TechDebtHub.Application.Common.Models;
using TechDebtHub.Application.Features.DividasTecnicas.BuscarDividaTecnicaPorId;
using TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica;
using TechDebtHub.Application.Features.DividasTecnicas.ListarDividasTecnicas;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Api.Controllers
{
    [ApiController]
    [Route("projetos/{projetoId:guid}/dividas")]
    public class DividasTecnicasController : ControllerBase
    {
        private readonly CriarDividaTecnicaHandler _criarDividaTecnicaHandler;
        private readonly BuscarDividaTecnicaPorIdHandler _buscarDividaTecnicaPorIdHandler;
        private readonly ListarDividasTecnicasHandler _listarDividasTecnicasHandler;

        public DividasTecnicasController(
            CriarDividaTecnicaHandler criarDividaTecnicaHandler,
            BuscarDividaTecnicaPorIdHandler buscarDividaTecnicaPorIdHandler,
            ListarDividasTecnicasHandler listarDividasTecnicasHandler
        )
        {
            _criarDividaTecnicaHandler = criarDividaTecnicaHandler;
            _buscarDividaTecnicaPorIdHandler = buscarDividaTecnicaPorIdHandler;
            _listarDividasTecnicasHandler = listarDividasTecnicasHandler;
        }

        [HttpPost]
        public async Task<ActionResult<CriarDividaTecnicaResponse>> Criar(
            Guid projetoId,
            CriarDividaTecnicaRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new CriarDividaTecnicaCommand(
                projetoId,
                request.Titulo,
                request.Descricao,
                request.Categoria,
                request.Impacto,
                request.Urgencia,
                request.Frequencia,
                request.Esforco
            );

            var response = await _criarDividaTecnicaHandler.HandleAsync(command, cancellationToken);

            return CreatedAtAction(nameof(BuscarPorId), new { id = response.Id }, response);
        }

        [HttpGet("/dividas/{id:guid}")]
        public async Task<ActionResult<BuscarDividaTecnicaPorIdResponse>> BuscarPorId(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var query = new BuscarDividaTecnicaPorIdQuery(id);

            var response = await _buscarDividaTecnicaPorIdHandler.HandleAsync(
                query,
                cancellationToken
            );

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ListarDividasTecnicasResponse>>> Listar(
            Guid projetoId,
            CancellationToken cancellationToken,
            [FromQuery] StatusDivida? status,
            [FromQuery] CategoriaDivida? categoria,
            [FromQuery] string? busca,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 10
        )
        {
            var query = new ListarDividasTecnicasQuery(
                projetoId,
                status,
                categoria,
                busca,
                pagina,
                tamanhoPagina
            );

            var response = await _listarDividasTecnicasHandler.HandleAsync(
                query,
                cancellationToken
            );

            return Ok(response);
        }
    }
}
