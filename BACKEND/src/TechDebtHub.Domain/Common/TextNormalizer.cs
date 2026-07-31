using System.Globalization;
using System.Text;

namespace TechDebtHub.Domain.Common;

public static class TextNormalizer
{
    public static string PrepararParaExibicao(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("O valor informado é obrigatório.", nameof(valor));
        }

        var partes = valor.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', partes);
    }

    public static string NormalizarParaComparacao(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ArgumentException("O valor informado é obrigatório.", nameof(valor));
        }

        var valorDecomposto = valor.Trim().Normalize(NormalizationForm.FormKD);

        var resultado = new StringBuilder(valorDecomposto.Length);
        var ultimoCaractereFoiEspaco = false;

        foreach (var caractere in valorDecomposto)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(caractere);

            var ehMarcaDeAcentuacao =
                categoria
                is UnicodeCategory.NonSpacingMark
                    or UnicodeCategory.SpacingCombiningMark
                    or UnicodeCategory.EnclosingMark;

            if (ehMarcaDeAcentuacao)
            {
                continue;
            }

            if (char.IsWhiteSpace(caractere))
            {
                if (resultado.Length > 0 && !ultimoCaractereFoiEspaco)
                {
                    resultado.Append(' ');
                    ultimoCaractereFoiEspaco = true;
                }

                continue;
            }

            resultado.Append(char.ToUpperInvariant(caractere));

            ultimoCaractereFoiEspaco = false;
        }

        return resultado.ToString().Trim().Normalize(NormalizationForm.FormC);
    }
}
