using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.Projetos.BuscarPorId;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.Projetos;

public sealed class BuscarPorIdHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_ProjetoExistente_DeveRetornarProjetoComMapeamentoCorreto()
    {
        // Given (Dado que) - Projeto persistido no banco de dados
        var projeto = ProjetoFactory.Criar(
            "TechDebt Hub",
            "Sistema para controle de dívidas técnicas."
        );

        Context.Projetos.Add(projeto);
        await Context.SaveChangesAsync();

        // Desconecta o ChangeTracker para forçar leitura real no SQLite
        Context.ChangeTracker.Clear();

        var handler = new BuscarPorIdHandler(Context);
        var query = new BuscarPorIdQuery(projeto.Id);

        // When (Quando) - Execução da consulta por ID
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // Then (Então) - Validação da projeção e mapeamento dos campos
        Assert.Equal(projeto.Id, response.Id);
        Assert.Equal(projeto.Nome, response.Nome);
        Assert.Equal(projeto.Descricao, response.Descricao);
        Assert.False(response.Arquivado);
        Assert.Equal(projeto.DataCriacao, response.DataCriacao);
    }

    [Fact]
    public async Task HandleAsync_ProjetoArquivado_DeveRetornarProjetoComStatusArquivado()
    {
        // Given (Dado que) - Projeto arquivado no banco
        var projeto = ProjetoFactory.Criar();
        projeto.Arquivar();

        Context.Projetos.Add(projeto);
        await Context.SaveChangesAsync();

        Context.ChangeTracker.Clear();

        var handler = new BuscarPorIdHandler(Context);
        var query = new BuscarPorIdQuery(projeto.Id);

        // When (Quando) - Execução da consulta por ID
        var response = await handler.HandleAsync(query, CancellationToken.None);

        // Then (Então) - O projeto deve ser retornado mantendo o status de arquivado
        Assert.Equal(projeto.Id, response.Id);
        Assert.True(response.Arquivado);
        Assert.NotNull(response.DataAtualizacao);
    }

    [Fact]
    public async Task HandleAsync_ProjetoInexistente_DeveLancarNotFoundException()
    {
        // Given (Dado que) - Um ID aleatório que não existe no banco
        var handler = new BuscarPorIdHandler(Context);
        var query = new BuscarPorIdQuery(Guid.NewGuid());

        // When (Quando) - Invocação do delegate de leitura
        Func<Task> acaoConsultaInexistente = () =>
            handler.HandleAsync(query, CancellationToken.None);

        // Then (Então) - Deve interromper e lançar NotFoundException
        var exception = await Assert.ThrowsAsync<NotFoundException>(acaoConsultaInexistente);
        Assert.Equal("Projeto não encontrado", exception.Message);
    }
}
