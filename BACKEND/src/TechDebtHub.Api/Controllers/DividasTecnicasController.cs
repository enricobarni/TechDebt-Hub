using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.DividasTecnicas;
using TechDebtHub.Application.Features.DividasTecnicas.BuscarDividaTecnicaPorId;
using TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica;

namespace TechDebtHub.Api.Controllers
{
    [ApiController]
    [Route("projetos/{projetoId:guid}/dividas")]
    public class DividasTecnicasController : ControllerBase
    {
        private readonly CriarDividaTecnicaHandler _criarDividaTecnicaHandler;
        private readonly BuscarDividaTecnicaPorIdHandler _buscarDividaTecnicaPorIdHandler;

        public DividasTecnicasController(
            CriarDividaTecnicaHandler criarDividaTecnicaHandler,
            BuscarDividaTecnicaPorIdHandler buscarDividaTecnicaPorIdHandler
        )
        {
            _criarDividaTecnicaHandler = criarDividaTecnicaHandler;
            _buscarDividaTecnicaPorIdHandler = buscarDividaTecnicaPorIdHandler;
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
    }
}
