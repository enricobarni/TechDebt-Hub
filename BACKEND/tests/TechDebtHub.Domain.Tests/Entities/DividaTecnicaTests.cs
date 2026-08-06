using System;
using TechDebtHub.Domain.Entities;
using TechDebtHub.Domain.Enums;
using TechDebtHub.Domain.Exceptions;
using TechDebtHub.Domain.Tests.Factories;
using Xunit;

namespace TechDebtHub.Domain.Tests.Entities;

public sealed class DividaTecnicaTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveCriarDividaAberta()
    {
        // Given (Dado que) - Instanciação através da Factory
        // Não há parâmetros manuais necessários

        // When (Quando) - Execução da criação
        var divida = DividaTecnicaFactory.Criar();

        // Then (Então) - Validações do estado inicial da Dívida Técnica
        Assert.NotEqual(Guid.Empty, divida.Id);
        Assert.NotEqual(Guid.Empty, divida.ProjetoId);
        Assert.Equal("Consulta sem paginação", divida.Titulo);
        Assert.Equal("CONSULTA SEM PAGINACAO", divida.TituloNormalizado);
        Assert.Equal(StatusDivida.Aberta, divida.Status);
        Assert.False(divida.Arquivada);
        Assert.Null(divida.DataAtualizacao);
        Assert.Null(divida.DataResolucao);
        Assert.True(divida.PontuacaoPrioridade > 0);
    }

    [Fact]
    public void Atualizar_DeveAtualizarDadosERecalcularPontuacao()
    {
        // Given - Dívida ativa criada e pontuação original registrada
        var divida = DividaTecnicaFactory.Criar();
        var pontuacaoAnterior = divida.PontuacaoPrioridade;

        // When - Atualização dos dados e recalculo de impacto/urgência
        divida.AtualizarDividaTecnica(
            "Endpoint sem cache",
            "A consulta deveria utilizar cache.",
            CategoriaDivida.Performance,
            NivelImpacto.Critico,
            NivelUrgencia.Alta,
            NivelFrequencia.Constante,
            NivelEsforco.Baixo
        );

        // Then - Validação dos dados atualizados e da mudança de pontuação
        Assert.Equal("Endpoint sem cache", divida.Titulo);
        Assert.Equal("ENDPOINT SEM CACHE", divida.TituloNormalizado);
        Assert.NotEqual(pontuacaoAnterior, divida.PontuacaoPrioridade);
        Assert.NotNull(divida.DataAtualizacao);
    }

    [Fact]
    public void Atualizar_DividaArquivada_DeveLancarExcecao()
    {
        // Given - Dívida criada e arquivada
        var divida = DividaTecnicaFactory.Criar();
        divida.Arquivar();

        // When - Tentativa de alteração em entidade arquivada
        Action acaoInvalida = () =>
            divida.AtualizarDividaTecnica(
                "Novo título",
                "Nova descrição.",
                CategoriaDivida.Performance,
                NivelImpacto.Alto,
                NivelUrgencia.Alta,
                NivelFrequencia.Constante,
                NivelEsforco.Medio
            );

        // Then - Validação da exceção de domínio
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("A dívida técnica já está arquivada", exception.Message);
    }

    [Fact]
    public void Atualizar_DividaResolvida_DeveLancarExcecao()
    {
        // Given - Dívida no estado terminal Resolvida
        var divida = CriarDividaResolvida();

        // When - Tentativa de alteração em entidade resolvida
        Action acaoInvalida = () =>
            divida.AtualizarDividaTecnica(
                "Novo título",
                "Nova descrição.",
                CategoriaDivida.Performance,
                NivelImpacto.Alto,
                NivelUrgencia.Alta,
                NivelFrequencia.Constante,
                NivelEsforco.Medio
            );

        // Then - Validação da regra de proteção contra edição de dívidas pagas
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("Não é possível alterar uma dívida técnica já resolvida", exception.Message);
    }

    [Fact]
    public void Arquivar_DividaAtiva_DeveArquivar()
    {
        // Given
        var divida = DividaTecnicaFactory.Criar();

        // When
        divida.Arquivar();

        // Then
        Assert.True(divida.Arquivada);
        Assert.NotNull(divida.DataAtualizacao);
    }

    [Fact]
    public void Arquivar_DividaResolvida_DeveLancarExcecao()
    {
        // Given - Dívida que foi percorrida até o status Resolvida
        var divida = CriarDividaResolvida();

        // When - Tentativa de arquivamento após resolução
        Action acaoInvalida = () => divida.Arquivar();

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("Não é possível arquivar uma dívida técnica resolvida", exception.Message);
    }

    [Fact]
    public void Arquivar_DividaJaArquivada_DeveLancarExcecao()
    {
        // Given - Dívida que já foi arquivada uma vez
        var divida = DividaTecnicaFactory.Criar();
        divida.Arquivar();

        // When - Segunda tentativa de arquivar
        Action segundaTentativaArquivar = () => divida.Arquivar();

        // Then
        var exception = Assert.Throws<DomainException>(segundaTentativaArquivar);
        Assert.Equal("A dívida técnica já está arquivada", exception.Message);
    }

    [Fact]
    public void AlterarStatus_MesmoStatus_DeveLancarExcecao()
    {
        var divida = DividaTecnicaFactory.Criar();

        var exception = Assert.Throws<DomainException>(() =>
            divida.AlterarStatus(StatusDivida.Aberta)
        );

        Assert.Equal("A dívida técnica já está nesse status", exception.Message);
    }

    [Fact]
    public void AlterarStatus_ParaResolvida_DevePreencherDataResolucao()
    {
        var divida = CriarDividaNoStatus(StatusDivida.EmAndamento);

        divida.AlterarStatus(StatusDivida.Resolvida);

        Assert.Equal(StatusDivida.Resolvida, divida.Status);
        Assert.NotNull(divida.DataResolucao);
        Assert.NotNull(divida.DataAtualizacao);
    }

    [Theory]
    [InlineData(StatusDivida.Aberta, StatusDivida.EmAnalise)]
    [InlineData(StatusDivida.EmAnalise, StatusDivida.Planejada)]
    [InlineData(StatusDivida.Planejada, StatusDivida.EmAndamento)]
    [InlineData(StatusDivida.EmAndamento, StatusDivida.Resolvida)]
    [InlineData(StatusDivida.Aberta, StatusDivida.Aceita)]
    [InlineData(StatusDivida.EmAnalise, StatusDivida.Aceita)]
    [InlineData(StatusDivida.Planejada, StatusDivida.Aceita)]
    public void AlterarStatus_TransicaoPermitida_DeveAlterarStatus(
        StatusDivida statusAtual,
        StatusDivida novoStatus
    )
    {
        var divida = CriarDividaNoStatus(statusAtual);

        divida.AlterarStatus(novoStatus);

        Assert.Equal(novoStatus, divida.Status);
        Assert.NotNull(divida.DataAtualizacao);
    }

    [Theory]
    [InlineData(StatusDivida.Aberta, StatusDivida.Resolvida)]
    [InlineData(StatusDivida.Aberta, StatusDivida.EmAndamento)]
    [InlineData(StatusDivida.EmAnalise, StatusDivida.Resolvida)]
    [InlineData(StatusDivida.Planejada, StatusDivida.Resolvida)]
    [InlineData(StatusDivida.EmAndamento, StatusDivida.EmAnalise)]
    public void AlterarStatus_TransicaoInvalida_DeveLancarExcecao(
        StatusDivida statusAtual,
        StatusDivida novoStatus
    )
    {
        var divida = CriarDividaNoStatus(statusAtual);

        Assert.Throws<DomainException>(() => divida.AlterarStatus(novoStatus));
    }

    [Fact]
    public void AlterarStatus_DividaArquivada_DeveLancarExcecao()
    {
        // Given - Dívida criada e arquivada
        var divida = DividaTecnicaFactory.Criar();
        divida.Arquivar();

        // When - Tentativa de alterar status de entidade arquivada
        Action acaoInvalida = () => divida.AlterarStatus(StatusDivida.EmAnalise);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("A dívida técnica já está arquivada", exception.Message);
    }

    [Theory]
    [InlineData(StatusDivida.Aberta)]
    [InlineData(StatusDivida.EmAnalise)]
    [InlineData(StatusDivida.Planejada)]
    [InlineData(StatusDivida.EmAndamento)]
    [InlineData(StatusDivida.Resolvida)]
    public void AlterarStatus_APartirDeAceita_DeveLancarExcecao(StatusDivida novoStatus)
    {
        // Given - Dívida no estado terminal Aceita
        var divida = CriarDividaAceita();

        // When - Tentativa de mudar para qualquer outro status
        Action acaoInvalida = () => divida.AlterarStatus(novoStatus);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal(
            $"Não é permitido alterar o status de {StatusDivida.Aceita} para {novoStatus}",
            exception.Message
        );
    }

    [Fact]
    public void AlterarStatus_NovoStatusForaDoEnum_DeveLancarExcecao()
    {
        // Given
        var divida = DividaTecnicaFactory.Criar();
        var statusInvalido = (StatusDivida)999;

        // When
        Action acaoInvalida = () => divida.AlterarStatus(statusInvalido);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("O valor informado para novoStatus é inválido.", exception.Message);
    }

    [Fact]
    public void Criar_ProjetoIdVazio_DeveLancarExcecao()
    {
        // Given
        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(projetoId: Guid.Empty);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("O projeto é obrigatório", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_TituloVazio_DeveLancarExcecao(string? tituloInvalido)
    {
        // Given
        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(titulo: tituloInvalido!);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("O título da dívida técnica é obrigatório", exception.Message);
    }

    [Fact]
    public void Criar_TituloMaiorQueLimitePermitido_DeveLancarExcecao()
    {
        // Given
        var tituloInvalido = new string('A', 121);

        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(titulo: tituloInvalido);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("O título deve possuir no máximo 120 caracteres.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_DescricaoVazia_DeveLancarExcecao(string? descricaoInvalida)
    {
        // Given
        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(descricao: descricaoInvalida!);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("A descrição da Dívida Técnica é obrigatória", exception.Message);
    }

    [Fact]
    public void Criar_DescricaoMaiorQueLimitePermitido_DeveLancarExcecao()
    {
        // Given
        var descricaoInvalida = new string('A', 2001);

        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(descricao: descricaoInvalida);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("A descrição deve possuir no máximo 2000 caracteres.", exception.Message);
    }

    [Fact]
    public void Criar_CategoriaInvalida_DeveLancarExcecao()
    {
        // Given
        var categoriaInvalida = (CategoriaDivida)999;

        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(categoria: categoriaInvalida);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("O valor informado para categoria é inválido.", exception.Message);
    }

    [Fact]
    public void Criar_ImpactoInvalido_DeveLancarExcecao()
    {
        // Given
        var impactoInvalido = (NivelImpacto)999;

        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(impacto: impactoInvalido);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("O valor informado para impacto é inválido.", exception.Message);
    }

    [Fact]
    public void Criar_UrgenciaInvalida_DeveLancarExcecao()
    {
        // Given
        var urgenciaInvalida = (NivelUrgencia)999;

        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(urgencia: urgenciaInvalida);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("O valor informado para urgencia é inválido.", exception.Message);
    }

    [Fact]
    public void Criar_FrequenciaInvalida_DeveLancarExcecao()
    {
        // Given
        var frequenciaInvalida = (NivelFrequencia)999;

        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(frequencia: frequenciaInvalida);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("O valor informado para frequencia é inválido.", exception.Message);
    }

    [Fact]
    public void Criar_EsforcoInvalido_DeveLancarExcecao()
    {
        // Given
        var esforcoInvalido = (NivelEsforco)999;

        // When
        Action acaoInvalida = () => DividaTecnicaFactory.Criar(esforco: esforcoInvalido);

        // Then
        var exception = Assert.Throws<DomainException>(acaoInvalida);
        Assert.Equal("O valor informado para esforco é inválido.", exception.Message);
    }

    // Helper privado para montagem de cenários de teste complexos
    private static DividaTecnica CriarDividaResolvida()
    {
        var divida = DividaTecnicaFactory.Criar();

        divida.AlterarStatus(StatusDivida.EmAnalise);
        divida.AlterarStatus(StatusDivida.Planejada);
        divida.AlterarStatus(StatusDivida.EmAndamento);
        divida.AlterarStatus(StatusDivida.Resolvida);

        return divida;
    }

    private static DividaTecnica CriarDividaAceita()
    {
        var divida = DividaTecnicaFactory.Criar();

        divida.AlterarStatus(StatusDivida.Aceita);

        return divida;
    }

    private static DividaTecnica CriarDividaNoStatus(StatusDivida statusDesejado)
    {
        var divida = DividaTecnicaFactory.Criar();

        if (statusDesejado == StatusDivida.Aberta)
        {
            return divida;
        }

        divida.AlterarStatus(StatusDivida.EmAnalise);

        if (statusDesejado == StatusDivida.EmAnalise)
        {
            return divida;
        }

        divida.AlterarStatus(StatusDivida.Planejada);

        if (statusDesejado == StatusDivida.Planejada)
        {
            return divida;
        }

        divida.AlterarStatus(StatusDivida.EmAndamento);

        if (statusDesejado == StatusDivida.EmAndamento)
        {
            return divida;
        }

        divida.AlterarStatus(StatusDivida.Resolvida);

        return divida;
    }
}
