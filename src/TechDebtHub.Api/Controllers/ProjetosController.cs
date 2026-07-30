using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.Projetos;
using TechDebtHub.Application.Features.Projetos.BuscarPorId;
using TechDebtHub.Application.Features.Projetos.CriarProjeto;

namespace TechDebtHub.Api.Controllers
{
    [ApiController]
    [Route("projetos")]
    public sealed class ProjetosController : ControllerBase
    {
        private readonly CriarProjetoHandler _criarProjetoHandler;
        private readonly BuscarPorIdHandler _buscarPorIdHandler;

        public ProjetosController(
            CriarProjetoHandler criarProjetoHandler,
            BuscarPorIdHandler buscarPorIdHandler
        )
        {
            _criarProjetoHandler = criarProjetoHandler;
            _buscarPorIdHandler = buscarPorIdHandler;
        }

        [HttpPost]
        public async Task<ActionResult<CriarProjetoResponse>> Criar(
            CriarProjetoRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new CriarProjetoCommand(request.Nome, request.Descricao);

            var response = await _criarProjetoHandler.HandleAsync(command, cancellationToken);

            return CreatedAtAction(nameof(BuscarPorId), new {id = response.Id}, response);
        }

        [HttpGet("{id:guide}")]
        public async Task<ActionResult<BuscarPorIdResponse>> BuscarPorId(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var query = new BuscarPorIdQuery(id);

            var response = await _buscarPorIdHandler.HandlerAsync(query, cancellationToken);

            return Ok(response);
        }
    }
}
