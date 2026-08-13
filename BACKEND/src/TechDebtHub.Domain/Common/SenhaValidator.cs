using System.Text.RegularExpressions;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Domain.Common;

public static class SenhaValidator
{
    public static void Validar(string senha)
    {
        int tamanhoMinimo = 8;
        int tamanhoMaximo = 128;

        if (string.IsNullOrWhiteSpace(senha))
        {
            throw new DomainException("A senha é obrigatória");
        }

        if (senha.Length < tamanhoMinimo)
        {
            throw new DomainException("A senha deve possuir pelo menos 8 caracteres");
        }

        if (senha.Length > tamanhoMaximo)
        {
            throw new DomainException("A senha deve possuir no máximo 128 caracteres");
        }

        if (!Regex.IsMatch(senha, @"[a-z]"))
        {
            throw new DomainException("A senha deve possuir pelo menos uma letra minúscula");
        }

        if (!Regex.IsMatch(senha, @"[A-Z]"))
        {
            throw new DomainException("A senha deve possuir pelo menos uma letra maiúscula");
        }

        if (!Regex.IsMatch(senha, @"[0-9]"))
        {
            throw new DomainException("A senha deve possuir pelo menos um número");
        }

        if (!Regex.IsMatch(senha, @"[^a-zA-Z0-9]"))
        {
            throw new DomainException("A senha deve possuir pelo menos um caractere especial");
        }
    }
}
