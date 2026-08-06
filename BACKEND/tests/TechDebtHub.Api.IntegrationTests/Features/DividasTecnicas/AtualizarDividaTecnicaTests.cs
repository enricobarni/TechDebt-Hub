using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Api.Contracts.DividasTecnicas;
using TechDebtHub.Api.IntegrationTests.Common;
using TechDebtHub.Application.Features.DividasTecnicas.AtualizarDividaTecnica;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Api.IntegrationTests.Features.DividasTecnicas;

public sealed class AtualizarDividaTecnicaTests : ApiIntegrationTestBase
{
    public AtualizarDividaTecnicaTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Put_DividasTecnicas_ComDadosValidos_DeveRetornar200EPersistirComPontuacaoRecalculada()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(
            projeto.Id,
            "Titulo inicial",
            "Descricao inicial.",
            CategoriaDivida.Performance,
            NivelImpacto.Baixo,
            NivelUrgencia.Baixa,
            NivelFrequencia.Rara,
            NivelEsforco.Alto
        );

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        var request = new AtualizarDividaTecnicaRequest(
            "Titulo atualizado",
            "Descricao atualizada.",
            CategoriaDivida.Seguranca,
            NivelImpacto.Critico,
            NivelUrgencia.Imediata,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        var response = await Client.PutAsJsonAsync($"/dividas/{divida.Id}", request);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var corpo = await response.Content.ReadFromJsonAsync<AtualizarDividaTecnicaResponse>();

        Assert.NotNull(corpo);
        Assert.Equal(divida.Id, corpo!.Id);
        Assert.Equal(projeto.Id, corpo.ProjetoId);
        Assert.Equal(request.Titulo, corpo.Titulo);
        Assert.Equal(request.Descricao, corpo.Descricao);
        Assert.Equal(request.Categoria, corpo.Categoria);
        Assert.Equal(request.Impacto, corpo.Impacto);
        Assert.Equal(request.Urgencia, corpo.Urgencia);
        Assert.Equal(request.Frequencia, corpo.Frequencia);
        Assert.Equal(request.Esforco, corpo.Esforco);
        Assert.NotNull(corpo.DataAtualizacao);

        // PontuacaoPrioridade = Impacto * Urgencia * Frequencia / Esforco = 4 * 4 * 4 / 2 = 32
        Assert.Equal(32m, corpo.PontuacaoPrioridade);

        var dividaPersistida = await UsingContextAsync(context =>
            context.DividasTecnicas.AsNoTracking().SingleAsync(d => d.Id == divida.Id)
        );

        Assert.Equal(request.Titulo, dividaPersistida.Titulo);
        Assert.Equal(request.Descricao, dividaPersistida.Descricao);
        Assert.Equal(32m, dividaPersistida.PontuacaoPrioridade);
        Assert.NotNull(dividaPersistida.DataAtualizacao);
    }

    [Fact]
    public async Task Put_DividasTecnicas_ComIdInexistente_DeveRetornar404()
    {
        // Given
        var idInexistente = Guid.NewGuid();

        var request = new AtualizarDividaTecnicaRequest(
            "Titulo atualizado",
            "Descricao atualizada.",
            CategoriaDivida.Seguranca,
            NivelImpacto.Critico,
            NivelUrgencia.Imediata,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        var response = await Client.PutAsJsonAsync($"/dividas/{idInexistente}", request);

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Dívida técnica não encontrada", corpo);
    }

    [Fact]
    public async Task Put_DividasTecnicas_ComTituloDuplicadoDeOutraDividaDoMesmoProjeto_DeveRetornar409()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var dividaUm = DividaTecnicaFactory.Criar(projeto.Id, "Consulta sem paginação");
        var dividaDois = DividaTecnicaFactory.Criar(projeto.Id, "Deploy manual sem pipeline");

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.AddRange(dividaUm, dividaDois);
            await context.SaveChangesAsync();
        });

        var request = new AtualizarDividaTecnicaRequest(
            "consulta   sem   paginação",
            "Descricao atualizada.",
            dividaDois.Categoria,
            dividaDois.Impacto,
            dividaDois.Urgencia,
            dividaDois.Frequencia,
            dividaDois.Esforco
        );

        // When
        var response = await Client.PutAsJsonAsync($"/dividas/{dividaDois.Id}", request);

        // Then
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Já existe outra dívida com esse título neste projeto", corpo);
    }

    [Fact]
    public async Task Put_DividasTecnicas_ComDividaArquivada_DeveRetornar422()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.Arquivar();

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        var request = new AtualizarDividaTecnicaRequest(
            "Titulo atualizado",
            "Descricao atualizada.",
            divida.Categoria,
            divida.Impacto,
            divida.Urgencia,
            divida.Frequencia,
            divida.Esforco
        );

        // When
        var response = await Client.PutAsJsonAsync($"/dividas/{divida.Id}", request);

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("A dívida técnica já está arquivada", corpo);
    }

    [Fact]
    public async Task Put_DividasTecnicas_ComDividaResolvida_DeveRetornar422()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        var divida = DividaTecnicaFactory.Criar(projeto.Id);
        divida.AlterarStatus(StatusDivida.EmAnalise);
        divida.AlterarStatus(StatusDivida.Planejada);
        divida.AlterarStatus(StatusDivida.EmAndamento);
        divida.AlterarStatus(StatusDivida.Resolvida);

        await UsingContextAsync(async context =>
        {
            context.Projetos.Add(projeto);
            context.DividasTecnicas.Add(divida);
            await context.SaveChangesAsync();
        });

        var request = new AtualizarDividaTecnicaRequest(
            "Titulo atualizado",
            "Descricao atualizada.",
            divida.Categoria,
            divida.Impacto,
            divida.Urgencia,
            divida.Frequencia,
            divida.Esforco
        );

        // When
        var response = await Client.PutAsJsonAsync($"/dividas/{divida.Id}", request);

        // Then
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        var corpo = await response.Content.ReadAsStringAsync();
        Assert.Contains("Não é possível alterar uma dívida técnica já resolvida", corpo);
    }
}
