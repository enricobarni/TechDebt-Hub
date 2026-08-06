using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.Projetos;
using TechDebtHub.Application.Common.Models;
using TechDebtHub.Application.Features.Projetos.ArquivarProjeto;
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
        private readonly ArquivarProjetoHandler _arquivarProjetoHandler;

        public ProjetosController(
            CriarProjetoHandler criarProjetoHandler,
            BuscarPorIdHandler buscarPorIdHandler,
            ListarProjetosHandler listarProjetosHandler,
            AtualizarProjetoHandler atualizarProjetoHandler,
            ArquivarProjetoHandler arquivarProjetoHandler
        )
        {
            _criarProjetoHandler = criarProjetoHandler;
            _buscarPorIdHandler = buscarPorIdHandler;
            _listarProjetosHandler = listarProjetosHandler;
            _atualizarProjetoHandler = atualizarProjetoHandler;
            _arquivarProjetoHandler = arquivarProjetoHandler;
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

            var response = await _buscarPorIdHandler.HandleAsync(query, cancellationToken);

            return Ok(response);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ProjetoResumoResponse>>> Listar(
            CancellationToken cancellationToken,
            [FromQuery] string? busca,
            [FromQuery] bool? arquivado,
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 10
        )
        {
            var query = new ListarProjetosQuery(busca, arquivado, pagina, tamanhoPagina);

            var response = await _listarProjetosHandler.HandleAsync(query, cancellationToken);

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

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Arquivar(Guid id, CancellationToken cancellationToken)
        {
            var command = new ArquivarProjetoCommand(id);

            await _arquivarProjetoHandler.HandleAsync(command, cancellationToken);

            return NoContent();
        }
    }
}
