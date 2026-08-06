using System.Net;
using System.Net.Http.Json;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Features.DividasTecnicas.BuscarDividaTecnicaPorId;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Api.IntegrationTests.Features.DividasTecnicas;

public sealed class BuscarDividaTecnicaPorIdTests : ApiIntegrationTestBase
{
    public BuscarDividaTecnicaPorIdTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Get_DividasTecnicas_ComIdExistente_DeveRetornar200ComDivida()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(
            projeto.Id,
            "Consulta sem paginação",
            "A consulta retorna todos os registros de uma vez.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        // When
        var response = await Client.GetAsync($"/dividas/{divida.Id}");

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<BuscarDividaTecnicaPorIdResponse>();

        Assert.NotNull(corpo);
        Assert.Equal(divida.Id, corpo!.Id);
        Assert.Equal(divida.ProjetoId, corpo.ProjetoId);
        Assert.Equal(divida.Titulo, corpo.Titulo);
        Assert.Equal(divida.Descricao, corpo.Descricao);
        Assert.Equal(divida.Categoria, corpo.Categoria);
        Assert.Equal(divida.Status, corpo.Status);
        Assert.Equal(divida.Arquivada, corpo.Arquivada);
        Assert.Equal(divida.Impacto, corpo.Impacto);
        Assert.Equal(divida.Urgencia, corpo.Urgencia);
        Assert.Equal(divida.Frequencia, corpo.Frequencia);
        Assert.Equal(divida.Esforco, corpo.Esforco);
        Assert.Equal(divida.PontuacaoPrioridade, corpo.PontuacaoPrioridade);
        Assert.Equal(divida.DataCriacao, corpo.DataCriacao);
        Assert.Null(corpo.DataAtualizacao);
        Assert.Null(corpo.DataResolucao);
    }

    [Fact]
    public async Task Get_DividasTecnicas_ComIdInexistente_DeveRetornar404()
    {
        // Given
        var idInexistente = Guid.NewGuid();

        // When
        var response = await Client.GetAsync($"/dividas/{idInexistente}");

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Dívida técnica não encontrada", corpo);
    }
}
