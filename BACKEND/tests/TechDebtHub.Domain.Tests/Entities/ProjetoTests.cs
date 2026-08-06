using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Entities;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Domain.Tests.Entities
{
    public sealed class ProjetoTests
    {
        [Fact]
        public void Criar_ComDadosValidos_DeveCriarProjetoAtivo()
        {
            // Given
            var nome = "TechDebt Hub";
            var descricao = "Sistema para controle de dívidas técnicas.";

            // When
            var projeto = Projeto.Criar(nome, descricao);

            // Then
            Assert.NotEqual(Guid.Empty, projeto.Id);
            Assert.Equal("TechDebt Hub", projeto.Nome);
            Assert.Equal("TECHDEBT HUB", projeto.NomeNormalizado);
            Assert.False(projeto.Arquivado);
            Assert.NotEqual(default, projeto.DataCriacao);
            Assert.Null(projeto.DataAtualizacao);
        }

        [Fact]
        public void Criar_ComEspacosExcedentes_DevePrepararNome()
        {
            // Given
            var nomeComEspacos = "  TechDebt    Hub  ";
            var descricao = "Descrição válida.";

            // When
            var projeto = Projeto.Criar(nomeComEspacos, descricao);

            // Then
            Assert.Equal("TechDebt Hub", projeto.Nome);
            Assert.Equal("TECHDEBT HUB", projeto.NomeNormalizado);
        }

        [Fact]
        public void Atualizar_ComDadosValidos_DeveAlterarProjeto()
        {
            // Given
            var projeto = Projeto.Criar("Nome inicial", "Descrição inicial.");
            var novoNome = "Nome atualizado";
            var novaDescricao = "Descrição atualizada.";

            // When - Execução da atualização
            projeto.AtualizarProjeto(novoNome, novaDescricao);

            // Then
            Assert.Equal("Nome atualizado", projeto.Nome);
            Assert.Equal("NOME ATUALIZADO", projeto.NomeNormalizado);
            Assert.Equal("Descrição atualizada.", projeto.Descricao);
            Assert.NotNull(projeto.DataAtualizacao);
        }

        [Fact]
        public void Atualizar_ProjetoArquivado_DeveLancarExcecao()
        {
            // Given
            var projeto = Projeto.Criar("TechDebt Hub", "Descrição válida.");
            projeto.Arquivar();

            // When
            Action acaoInvalida = () => projeto.AtualizarProjeto("Outro nome", "Outra descrição.");

            // Then
            var exception = Assert.Throws<DomainException>(acaoInvalida);
            Assert.Equal("Não é possível alterar um projeto arquivado", exception.Message);
        }

        [Fact]
        public void Arquivar_ProjetoAtivo_DeveArquivar()
        {
            // Given
            var projeto = Projeto.Criar("TechDebt Hub", "Descrição válida.");

            // When
            projeto.Arquivar();

            // Then
            Assert.True(projeto.Arquivado);
            Assert.NotNull(projeto.DataAtualizacao);
        }

        [Fact]
        public void Arquivar_ProjetoJaArquivado_DeveLancarExcecao()
        {
            // Given
            var projeto = Projeto.Criar("TechDebt Hub", "Descrição válida.");
            projeto.Arquivar();

            // When
            Action segundaTentativaArquivar = () => projeto.Arquivar();

            // Then
            var exception = Assert.Throws<DomainException>(segundaTentativaArquivar);
            Assert.Equal("O projeto já está arquivado", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Criar_NomeVazio_DeveLancarExcecao(string? nomeInvalido)
        {
            // Given
            var descricao = "Descrição válida.";

            // When
            Action acaoInvalida = () => Projeto.Criar(nomeInvalido!, descricao);

            // Then
            var exception = Assert.Throws<DomainException>(acaoInvalida);
            Assert.Equal("O nome do projeto é obrigatório", exception.Message);
        }

        [Fact]
        public void Criar_NomeMaiorQueLimitePermitido_DeveLancarExcecao()
        {
            // Given
            var nomeInvalido = new string('A', 101);
            var descricao = "Descrição válida.";

            // When
            Action acaoInvalida = () => Projeto.Criar(nomeInvalido, descricao);

            // Then
            var exception = Assert.Throws<DomainException>(acaoInvalida);
            Assert.Equal("O nome do projeto deve possuir no máximo 100 caracteres", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Criar_DescricaoVazia_DeveLancarExcecao(string? descricaoInvalida)
        {
            // Given
            var nome = "TechDebt Hub";

            // When
            Action acaoInvalida = () => Projeto.Criar(nome, descricaoInvalida!);

            // Then
            var exception = Assert.Throws<DomainException>(acaoInvalida);
            Assert.Equal("A descrição do projeto é obrigatória", exception.Message);
        }

        [Fact]
        public void Criar_DescricaoMaiorQueLimitePermitido_DeveLancarExcecao()
        {
            // Given
            var nome = "TechDebt Hub";
            var descricaoInvalida = new string('A', 1001);

            // When
            Action acaoInvalida = () => Projeto.Criar(nome, descricaoInvalida);

            // Then
            var exception = Assert.Throws<DomainException>(acaoInvalida);
            Assert.Equal(
                "A descrição do projeto deve possuir no máximo 1000 caracteres",
                exception.Message
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Atualizar_NomeInvalido_DeveLancarExcecao(string? nomeInvalido)
        {
            // Given
            var projeto = Projeto.Criar("TechDebt Hub", "Descrição válida.");

            // When
            Action acaoInvalida = () => projeto.AtualizarProjeto(nomeInvalido!, "Outra descrição.");

            // Then
            var exception = Assert.Throws<DomainException>(acaoInvalida);
            Assert.Equal("O nome do projeto é obrigatório", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Atualizar_DescricaoInvalida_DeveLancarExcecao(string? descricaoInvalida)
        {
            // Given
            var projeto = Projeto.Criar("TechDebt Hub", "Descrição válida.");

            // When
            Action acaoInvalida = () => projeto.AtualizarProjeto("Outro nome", descricaoInvalida!);

            // Then
            var exception = Assert.Throws<DomainException>(acaoInvalida);
            Assert.Equal("A descrição do projeto é obrigatória", exception.Message);
        }
    }
}
