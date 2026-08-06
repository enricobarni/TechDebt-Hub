using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Common;

namespace TechDebtHub.Domain.Tests.Common
{
    public sealed class TextNormalizerTests
    {
        [Theory]
        [InlineData("TechDebt Hub", "TECHDEBT HUB")]
        [InlineData("techdebt hub", "TECHDEBT HUB")]
        [InlineData("  TechDebt Hub  ", "TECHDEBT HUB")]
        [InlineData("TechDebt     Hub", "TECHDEBT HUB")]
        [InlineData("TéchDébt Hüb", "TECHDEBT HUB")]
        [InlineData("Gestão Técnica", "GESTAO TECNICA")]
        [InlineData("Gestao Tecnica", "GESTAO TECNICA")]
        [InlineData("Tech-Debt", "TECH-DEBT")]
        [InlineData("C# API", "C# API")]
        public void NormalizarParaComparacao_DeveRetornarValorEsperado(
            string valor,
            string esperado
        )
        {
            var resultado = TextNormalizer.NormalizarParaComparacao(valor);

            Assert.Equal(esperado, resultado);
        }

        [Theory]
        [InlineData("  TechDebt Hub  ", "TechDebt Hub")]
        [InlineData("TechDebt     Hub", "TechDebt Hub")]
        [InlineData("TechDebt\tHub", "TechDebt Hub")]
        [InlineData("TechDebt\nHub", "TechDebt Hub")]
        public void NormalizarParaComparacao_DeveRemoverEspacosExcedentes(
            string valor,
            string esperado
        )
        {
            var resultado = TextNormalizer.PrepararParaExibicao(valor);

            Assert.Equal(esperado, resultado);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NormalizarParaComparacao_ValorVazio_DeveLancarExcecao(string? valor)
        {
            Assert.Throws<ArgumentException>(() => TextNormalizer.NormalizarParaComparacao(valor!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void PrepararParaExibicao_ValorVazio_DeveLancarExcecao(string? valor)
        {
            Assert.Throws<ArgumentException>(() => TextNormalizer.PrepararParaExibicao(valor!));
        }
    }
}
