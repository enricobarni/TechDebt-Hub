using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.DividasTecnicas;
using TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica;

namespace TechDebtHub.Api.Controllers
{
    [ApiController]
    [Route("projetos/{projetoId:guid}/dividas")]
    public class DividasTecnicasController : ControllerBase
    {
        private readonly CriarDividaTecnicaHandler _criarDividaTecnicaHandler;

        public DividasTecnicasController(CriarDividaTecnicaHandler criarDividaTecnicaHandler)
        {
            _criarDividaTecnicaHandler = criarDividaTecnicaHandler;
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

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
