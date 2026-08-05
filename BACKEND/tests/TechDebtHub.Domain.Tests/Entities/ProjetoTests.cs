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
            // Given (Dado que) - Preparação das entradas
            var nome = "TechDebt Hub";
            var descricao = "Sistema para controle de dívidas técnicas.";

            // When (Quando) - Execução do comportamento testado
            var projeto = Projeto.Criar(nome, descricao);

            // Then (Então) - Validação das asserções
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
            // Given - Criação do estado inicial do projeto
            var projeto = Projeto.Criar("Nome inicial", "Descrição inicial.");
            var novoNome = "Nome atualizado";
            var novaDescricao = "Descrição atualizada.";

            // When - Execução da atualização
            projeto.AtualizarProjeto(novoNome, novaDescricao);

            // Then - Validação das alterações efetuadas
            Assert.Equal("Nome atualizado", projeto.Nome);
            Assert.Equal("NOME ATUALIZADO", projeto.NomeNormalizado);
            Assert.Equal("Descrição atualizada.", projeto.Descricao);
            Assert.NotNull(projeto.DataAtualizacao);
        }

        [Fact]
        public void Atualizar_ProjetoArquivado_DeveLancarExcecao()
        {
            // Given - Projeto é criado e em seguida arquivado
            var projeto = Projeto.Criar("TechDebt Hub", "Descrição válida.");
            projeto.Arquivar();

            // When - Ação que deve falhar isolada em uma expressão
            Action acaoInvalida = () => projeto.AtualizarProjeto("Outro nome", "Outra descrição.");

            // Then - Validação da exceção e da mensagem de erro
            var exception = Assert.Throws<DomainException>(acaoInvalida);
            Assert.Equal("Não é possível alterar um projeto arquivado.", exception.Message);
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
            // Given - Projeto já inicia arquivado
            var projeto = Projeto.Criar("TechDebt Hub", "Descrição válida.");
            projeto.Arquivar();

            // When - Segunda tentativa de arquivar
            Action segundaTentativaArquivar = () => projeto.Arquivar();

            // Then
            var exception = Assert.Throws<DomainException>(segundaTentativaArquivar);
            Assert.Equal("O projeto já está arquivado.", exception.Message);
        }
    }
}
