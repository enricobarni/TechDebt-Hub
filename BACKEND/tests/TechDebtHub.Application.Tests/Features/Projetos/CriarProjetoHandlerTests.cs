using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.Projetos.CriarProjeto;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;

namespace TechDebtHub.Application.Tests.Features.Projetos
{
    public sealed class CriarProjetoHandlerTests : ApplicationTestBase
    {
        [Fact]
        public async Task HandleAsync_ComDadosValidos_DeveCriarEPersistirProjeto()
        {
            // Given
            var handler = new CriarProjetoHandler(Context);

            var command = new CriarProjetoCommand(
                Nome: "TechDebt Hub",
                Descricao: "Sistema para controle de dívidas técnicas."
            );

            // When
            var response = await handler.HandleAsync(command, CancellationToken.None);

            // Then
            var projetoSalvo = await Context
                .Projetos.AsNoTracking()
                .SingleAsync(p => p.Id == response.Id);

            Assert.NotEqual(Guid.Empty, response.Id);

            Assert.Equal(command.Nome, projetoSalvo.Nome);
            Assert.Equal("TECHDEBT HUB", projetoSalvo.NomeNormalizado);
            Assert.Equal(command.Descricao, projetoSalvo.Descricao);
            Assert.False(projetoSalvo.Arquivado);
            Assert.NotEqual(default, projetoSalvo.DataCriacao);
        }

        [Fact]
        public async Task HandleAsync_NomeDuplicado_DeveLancarConflictException()
        {
            // Given
            var projetoExistente = ProjetoFactory.Criar("TechDebt Hub");

            Context.Projetos.Add(projetoExistente);
            await Context.SaveChangesAsync();

            var handler = new CriarProjetoHandler(Context);

            var command = new CriarProjetoCommand(
                Nome: "techdebt    hub",
                Descricao: "Outro projeto com o mesmo nome normalizado."
            );

            // When
            Func<Task> acaoDuplicada = () => handler.HandleAsync(command, CancellationToken.None);

            // Then
            var exception = await Assert.ThrowsAsync<ConflictException>(acaoDuplicada);

            Assert.Equal("Já existe um projeto com esse nome", exception.Message);

            Assert.Equal(1, await Context.Projetos.CountAsync());
        }

        [Fact]
        public async Task HandleAsync_NomeDuplicadoSemAcentos_DeveLancarConflictException()
        {
            // Given
            var projetoExistente = ProjetoFactory.Criar("Gestão Técnica");

            Context.Projetos.Add(projetoExistente);
            await Context.SaveChangesAsync();

            var handler = new CriarProjetoHandler(Context);

            var command = new CriarProjetoCommand(
                Nome: "Gestao Tecnica",
                Descricao: "Outro projeto com o mesmo nome sem acentos."
            );

            // When
            Func<Task> acaoDuplicada = () => handler.HandleAsync(command, CancellationToken.None);

            // Then
            var exception = await Assert.ThrowsAsync<ConflictException>(acaoDuplicada);

            Assert.Equal("Já existe um projeto com esse nome", exception.Message);
            Assert.Equal(1, await Context.Projetos.CountAsync());
        }

        [Fact]
        public async Task Banco_NomeNormalizadoDuplicado_DeveRejeitarEThrowDbUpdateException()
        {
            // Given
            var projeto1 = ProjetoFactory.Criar("TechDebt Hub");
            var projeto2 = ProjetoFactory.Criar("techdebt hub");

            Context.Projetos.Add(projeto1);
            Context.Projetos.Add(projeto2);

            // When
            Func<Task> acaoSalvarDuplicado = () => Context.SaveChangesAsync();

            // Then
            await Assert.ThrowsAsync<DbUpdateException>(acaoSalvarDuplicado);
        }
    }
}
