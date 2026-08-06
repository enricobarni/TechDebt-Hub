using System.Net;
using System.Net.Http.Json;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Features.Projetos.BuscarPorId;
using TechDebtHub.Application.Tests.Factories;

namespace TechDebtHub.Api.IntegrationTests.Features.Projetos;

public sealed class BuscarPorIdTests : ApiIntegrationTestBase
{
    public BuscarPorIdTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Get_Projetos_ComIdExistente_DeveRetornar200ComProjeto()
    {
        // Given
        var projeto = ProjetoFactory.Criar(
            "TechDebt Hub",
            "Sistema para controle de dívidas técnicas."
        );

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync($"/projetos/{projeto.Id}");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<BuscarPorIdResponse>();

        Assert.NotNull(corpo);
        Assert.Equal(projeto.Id, corpo!.Id);
        Assert.Equal(projeto.Nome, corpo.Nome);
        Assert.Equal(projeto.Descricao, corpo.Descricao);
        Assert.Equal(projeto.DataCriacao, corpo.DataCriacao);
        Assert.Null(corpo.DataAtualizacao);
        Assert.False(corpo.Arquivado);
    }

    [Fact]
    public async Task Get_Projetos_ComIdInexistente_DeveRetornar404()
    {
        // Given
        var idInexistente = Guid.NewGuid();

        // When
        var response = await Client.GetAsync($"/projetos/{idInexistente}");

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Projeto não encontrado", corpo);
    }
}
