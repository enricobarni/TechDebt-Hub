using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.Usuarios;
using TechDebtHub.Application.Features.Usuarios.CadastrarUsuario;

namespace TechDebtHub.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly CadastrarUsuarioHandler _cadastrarUsuarioHandler;

        public AuthController(CadastrarUsuarioHandler cadastrarUsuarioHandler)
        {
            _cadastrarUsuarioHandler = cadastrarUsuarioHandler;
        }

        [HttpPost("register")]
        public async Task<ActionResult<CadastrarUsuarioResponse>> Cadastrar(
            [FromBody] CadastrarUsuarioRequest request,
            [FromServices] CadastrarUsuarioHandler handler,
            CancellationToken cancellationToken
        )
        {
            var command = new CadastrarUsuarioCommand(request.Nome, request.Email, request.Senha);

            var response = await _cadastrarUsuarioHandler.HandleAsync(command, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
