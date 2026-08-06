using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Api.Contracts.Projetos;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Features.Projetos.AtualizarProjeto;
using TechDebtHub.Application.Tests.Factories;

namespace TechDebtHub.Api.IntegrationTests.Features.Projetos;

public sealed class AtualizarProjetoTests : ApiIntegrationTestBase
{
    public AtualizarProjetoTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Put_Projetos_ComDadosValidos_DeveRetornar200EPersistirAlteracoes()
    {
        // Given
        var projeto = ProjetoFactory.Criar("Nome inicial", "Descrição inicial.");

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();
        });

        var request = new AtualizarProjetoRequest("Nome atualizado", "Descrição atualizada.");

        // When
        var response = await Client.PutAsJsonAsync($"/projetos/{projeto.Id}", request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<AtualizarProjetoResponse>();

        Assert.NotNull(corpo);
        Assert.Equal(projeto.Id, corpo!.Id);
        Assert.Equal(request.Nome, corpo.Nome);
        Assert.Equal(request.Descricao, corpo.Descricao);
        Assert.NotNull(corpo.DataAtualizacao);

        var projetoPersistido = await UsingContextAsync(context =>
            context.Projetos.AsNoTracking().SingleAsync(p => p.Id == projeto.Id)
        );

        Assert.Equal(request.Nome, projetoPersistido.Nome);
        Assert.Equal(request.Descricao, projetoPersistido.Descricao);
        Assert.NotNull(projetoPersistido.DataAtualizacao);
    }

    [Fact]
    public async Task Put_Projetos_ComIdInexistente_DeveRetornar404()
    {
        // Given
        var idInexistente = Guid.NewGuid();
        var request = new AtualizarProjetoRequest("Nome atualizado", "Descrição atualizada.");

        // When
        var response = await Client.PutAsJsonAsync($"/projetos/{idInexistente}", request);

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_Projetos_ComNomeDuplicadoDeOutroProjeto_DeveRetornar409()
    {
        // Given
        var projetoUm = ProjetoFactory.Criar("TechDebt Hub");
        var projetoDois = ProjetoFactory.Criar("Sistema Financeiro");

        await UsingContextAsync(async context =>
        {
            context.Projetos.AddRange(projetoUm, projetoDois);
            await context.SaveChangesAsync();
        });

        var request = new AtualizarProjetoRequest("techdebt   hub", "Descrição atualizada.");

        // When
        var response = await Client.PutAsJsonAsync($"/projetos/{projetoDois.Id}", request);

        // Then
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Já existe um projeto com esse nome", corpo);
    }

    [Fact]
    public async Task Put_Projetos_ComProjetoArquivado_DeveRetornar422()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        projeto.Arquivar();

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();
        });

        var request = new AtualizarProjetoRequest("Novo nome", "Nova descrição.");

        // When
        var response = await Client.PutAsJsonAsync($"/projetos/{projeto.Id}", request);

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Não é possível alterar um projeto arquivado", corpo);
    }
}
