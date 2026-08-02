using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.DividasTecnicas;
using TechDebtHub.Application.Common.Models;
using TechDebtHub.Application.Features.DividasTecnicas.AlterarStatusDividaTecnica;
using TechDebtHub.Application.Features.DividasTecnicas.ArquivarDividaTecnica;
using TechDebtHub.Application.Features.DividasTecnicas.AtualizarDividaTecnica;
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
        private readonly AtualizarDividaTecnicaHandler _atualizarDividaTecnicaHandler;
        private readonly AlterarStatusDividaTecnicaHandler _alterarStatusDividaTecnicaHandler;
        private readonly ArquivarDividaTecnicaHandler _arquivarDividaTecnicaHandler;

        public DividasTecnicasController(
            CriarDividaTecnicaHandler criarDividaTecnicaHandler,
            BuscarDividaTecnicaPorIdHandler buscarDividaTecnicaPorIdHandler,
            ListarDividasTecnicasHandler listarDividasTecnicasHandler,
            AtualizarDividaTecnicaHandler atualizarDividaTecnicaHandler,
            AlterarStatusDividaTecnicaHandler alterarStatusDividaTecnicaHandler,
            ArquivarDividaTecnicaHandler arquivarDividaTecnicaHandler
        )
        {
            _criarDividaTecnicaHandler = criarDividaTecnicaHandler;
            _buscarDividaTecnicaPorIdHandler = buscarDividaTecnicaPorIdHandler;
            _listarDividasTecnicasHandler = listarDividasTecnicasHandler;
            _atualizarDividaTecnicaHandler = atualizarDividaTecnicaHandler;
            _alterarStatusDividaTecnicaHandler = alterarStatusDividaTecnicaHandler;
            _arquivarDividaTecnicaHandler = arquivarDividaTecnicaHandler;
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
            [FromQuery] bool? arquivada,
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
                arquivada,
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

        [HttpPut("/dividas/{id:guid}")]
        public async Task<ActionResult<AtualizarDividaTecnicaResponse>> Atualizar(
            Guid id,
            AtualizarDividaTecnicaRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new AtualizarDividaTecnicaCommand(
                id,
                request.Titulo,
                request.Descricao,
                request.Categoria,
                request.Impacto,
                request.Urgencia,
                request.Frequencia,
                request.Esforco
            );

            var response = await _atualizarDividaTecnicaHandler.HandleAsync(
                command,
                cancellationToken
            );

            return Ok(response);
        }

        [HttpPatch("/dividas/{id:guid}/status")]
        public async Task<ActionResult<AlterarStatusDividaTecnicaResponse>> AlterarStatus(
            Guid id,
            AlterarStatusDividaTecnicaRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new AlterarStatusDividaTecnicaCommand(id, request.NovoStatus);

            var responde = await _alterarStatusDividaTecnicaHandler.HandleAsync(
                command,
                cancellationToken
            );

            return Ok(responde);
        }

        [HttpDelete("/dividas{id:guid}")]
        public async Task<IActionResult> Arquivar(Guid id, CancellationToken cancellationToken)
        {
            var command = new ArquivarDividaTecnicaCommand(id);

            await _arquivarDividaTecnicaHandler.HandleAsync(command, cancellationToken);

            return NoContent();
        }
    }
}
