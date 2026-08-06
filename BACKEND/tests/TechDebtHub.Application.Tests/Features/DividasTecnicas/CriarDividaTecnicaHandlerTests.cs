using Microsoft.EntityFrameworkCore;
using TechDebtHub.Application.Exceptions;
using TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica;
using TechDebtHub.Application.Tests.Common;
using TechDebtHub.Application.Tests.Factories;
using TechDebtHub.Domain.Enums;
using TechDebtHub.Domain.Exceptions;
using Xunit;

namespace TechDebtHub.Application.Tests.Features.DividasTecnicas;

public sealed class CriarDividaTecnicaHandlerTests : ApplicationTestBase
{
    [Fact]
    public async Task HandleAsync_ProjetoInexistente_DeveLancarNotFoundException()
    {
        // Given
        var handler = new CriarDividaTecnicaHandler(Context);
        var command = new CriarDividaTecnicaCommand(
            Guid.NewGuid(),
            "Consulta sem paginação",
            "A consulta retorna todos os registros.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        Func<Task> acaoProjetoInexistente = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<NotFoundException>(acaoProjetoInexistente);
        Assert.Equal("Projeto não encontrado", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_ProjetoArquivado_DeveLancarDomainException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        projeto.Arquivar();

        Context.Projetos.Add(projeto);
        await Context.SaveChangesAsync();

        var handler = new CriarDividaTecnicaHandler(Context);
        var command = new CriarDividaTecnicaCommand(
            projeto.Id,
            "Consulta sem paginação",
            "A consulta retorna todos os registros.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        Func<Task> acaoProjetoArquivado = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<DomainException>(acaoProjetoArquivado);
        Assert.Equal(
            "Não é possível criar uma dívida em um projeto arquivado.",
            exception.Message
        );
    }

    [Fact]
    public async Task HandleAsync_TituloDuplicadoNoMesmoProjeto_DeveLancarConflictException()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);

        var dividaExistente = DividaTecnicaFactory.Criar(projeto.Id, titulo: "Consulta lenta");
        Context.DividasTecnicas.Add(dividaExistente);

        await Context.SaveChangesAsync();

        var handler = new CriarDividaTecnicaHandler(Context);
        var command = new CriarDividaTecnicaCommand(
            projeto.Id,
            "consulta    lenta",
            "Outra descrição para o mesmo título normalizado.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        Func<Task> acaoTituloDuplicado = () =>
            handler.HandleAsync(command, CancellationToken.None);

        // Then
        var exception = await Assert.ThrowsAsync<ConflictException>(acaoTituloDuplicado);
        Assert.Equal(
            "Já existe uma dívida com esse título neste projeto.",
            exception.Message
        );

        Assert.Equal(1, await Context.DividasTecnicas.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_TituloIgualEmProjetoDiferente_DeveCriarComSucesso()
    {
        // Given
        var projetoUm = ProjetoFactory.Criar("Projeto Um");
        var projetoDois = ProjetoFactory.Criar("Projeto Dois");
        Context.Projetos.AddRange(projetoUm, projetoDois);

        var dividaNoProjetoUm = DividaTecnicaFactory.Criar(
            projetoUm.Id,
            titulo: "Título Repetido"
        );
        Context.DividasTecnicas.Add(dividaNoProjetoUm);

        await Context.SaveChangesAsync();

        var handler = new CriarDividaTecnicaHandler(Context);
        var command = new CriarDividaTecnicaCommand(
            projetoDois.Id,
            "Título Repetido",
            "Dívida técnica em outro projeto com o mesmo título.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Media,
            NivelFrequencia.Constante,
            NivelEsforco.Medio
        );

        // When
        var response = await handler.HandleAsync(command, CancellationToken.None);

        // Then
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(projetoDois.Id, response.ProjetoId);
        Assert.Equal(2, await Context.DividasTecnicas.CountAsync());
    }

    [Fact]
    public async Task HandleAsync_ComDadosValidos_DeveCriarEPersistirDividaTecnica()
    {
        // Given
        var projeto = ProjetoFactory.Criar();
        Context.Projetos.Add(projeto);
        await Context.SaveChangesAsync();

        var handler = new CriarDividaTecnicaHandler(Context);
        var command = new CriarDividaTecnicaCommand(
            projeto.Id,
            "Consulta sem paginação",
            "A consulta retorna todos os registros.",
            CategoriaDivida.Performance,
            NivelImpacto.Alto,
            NivelUrgencia.Alta,
            NivelFrequencia.Frequente,
            NivelEsforco.Baixo
        );

        // When
        var response = await handler.HandleAsync(command, CancellationToken.None);

        Context.ChangeTracker.Clear();

        // Then
        var dividaSalva = await Context
            .DividasTecnicas.AsNoTracking()
            .SingleAsync(divida => divida.Id == response.Id);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(projeto.Id, dividaSalva.ProjetoId);
        Assert.Equal(command.Titulo, dividaSalva.Titulo);
        Assert.Equal(command.Descricao, dividaSalva.Descricao);
        Assert.Equal(StatusDivida.Aberta, dividaSalva.Status);
        Assert.False(dividaSalva.Arquivada);

        // NivelImpacto.Alto (3) * NivelUrgencia.Alta (3) * NivelFrequencia.Frequente (3) / NivelEsforco.Baixo (1)
        Assert.Equal(27, dividaSalva.PontuacaoPrioridade);
        Assert.NotEqual(default, dividaSalva.DataCriacao);
    }
}
