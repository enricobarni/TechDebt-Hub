using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.Projetos;
using TechDebtHub.Application.Features.Projetos.AtualizarProjeto;
using TechDebtHub.Application.Features.Projetos.BuscarPorId;
using TechDebtHub.Application.Features.Projetos.CriarProjeto;
using TechDebtHub.Application.Features.Projetos.ListarProjetos;

namespace TechDebtHub.Api.Controllers
{
    [ApiController]
    [Route("projetos")]
    public sealed class ProjetosController : ControllerBase
    {
        private readonly CriarProjetoHandler _criarProjetoHandler;
        private readonly BuscarPorIdHandler _buscarPorIdHandler;
        private readonly ListarProjetosHandler _listarProjetosHandler;
        private readonly AtualizarProjetoHandler _atualizarProjetoHandler;

        public ProjetosController(
            CriarProjetoHandler criarProjetoHandler,
            BuscarPorIdHandler buscarPorIdHandler,
            ListarProjetosHandler listarProjetosHandler,
            AtualizarProjetoHandler atualizarProjetoHandler
        )
        {
            _criarProjetoHandler = criarProjetoHandler;
            _buscarPorIdHandler = buscarPorIdHandler;
            _listarProjetosHandler = listarProjetosHandler;
            _atualizarProjetoHandler = atualizarProjetoHandler;
        }

        [HttpPost]
        public async Task<ActionResult<CriarProjetoResponse>> Criar(
            CriarProjetoRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new CriarProjetoCommand(request.Nome, request.Descricao);

            var response = await _criarProjetoHandler.HandleAsync(command, cancellationToken);

            return CreatedAtAction(nameof(BuscarPorId), new { id = response.Id }, response);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BuscarPorIdResponse>> BuscarPorId(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            var query = new BuscarPorIdQuery(id);

            var response = await _buscarPorIdHandler.HandlerAsync(query, cancellationToken);

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProjetoResumoResponse>>> Listar(
            CancellationToken cancellationToken
        )
        {
            var query = new ListarProjetosQuery();

            var response = await _listarProjetosHandler.HandlerAsync(query, cancellationToken);

            return Ok(response);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<AtualizarProjetoResponse>> Atualizar(
            Guid id,
            AtualizarProjetoRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new AtualizarProjetoCommand(id, request.Nome, request.Descricao);

            var response = await _atualizarProjetoHandler.HandleAsync(command, cancellationToken);

            return Ok(response);
        }
    }
}
