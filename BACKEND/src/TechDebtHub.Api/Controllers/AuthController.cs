using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TechDebtHub.Api.Contracts.Auth;
using TechDebtHub.Api.Contracts.Usuarios;
using TechDebtHub.Application.Features.Usuarios.CadastrarUsuario;
using TechDebtHub.Application.Features.Usuarios.Login;
using TechDebtHub.Application.Features.Usuarios.RefreshTokens;

namespace TechDebtHub.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly CadastrarUsuarioHandler _cadastrarUsuarioHandler;
        private readonly LoginHandler _loginHandler;
        private readonly RefreshTokensHandler _refreshTokensHandler;

        public AuthController(
            CadastrarUsuarioHandler cadastrarUsuarioHandler,
            LoginHandler loginHandler,
            RefreshTokensHandler refreshTokensHandler
        )
        {
            _cadastrarUsuarioHandler = cadastrarUsuarioHandler;
            _loginHandler = loginHandler;
            _refreshTokensHandler = refreshTokensHandler;
        }

        [HttpPost("register")]
        public async Task<ActionResult<CadastrarUsuarioResponse>> Cadastrar(
            CadastrarUsuarioRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new CadastrarUsuarioCommand(request.Nome, request.Email, request.Senha);

            var response = await _cadastrarUsuarioHandler.HandleAsync(command, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(
            LoginRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new LoginCommand(request.Email, request.Senha);

            var response = await _loginHandler.HandleAsync(command, cancellationToken);

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshTokensResponse>> Refresh(
            RefreshTokensRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new RefreshTokensCommand(request.RefreshToken);

            var response = await _refreshTokensHandler.HandleAsync(command, cancellationToken);

            return Ok(response);
        }
    }
}
