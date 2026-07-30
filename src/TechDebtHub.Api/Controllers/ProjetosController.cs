using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.Projetos;
using TechDebtHub.Application.Features.CriarProjeto;

namespace TechDebtHub.Api.Controllers
{
    [ApiController]
    [Route("projetos")]
    public sealed class ProjetosController : ControllerBase
    {
        private readonly CriarProjetoHandler _criarProjetoHandler;

        public ProjetosController(CriarProjetoHandler criarProjetoHandler)
        {
            _criarProjetoHandler = criarProjetoHandler;
        }

        [HttpPost]
        public async Task<ActionResult<CriarProjetoResponse>> Criar(
            CriarProjetoRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new CriarProjetoCommand(request.Nome, request.Descricao);

            var response = await _criarProjetoHandler.HandleAsync(command, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
