using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Domain.Entities
{
    public class CodigoConfirmacaoEmail
    {
        public Guid Id { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string CodigoHash { get; private set; } = string.Empty;
        public DateTime DataCriacao { get; private set; }
        public DateTime DataExpiracao { get; private set; }
        public DateTime? DataUtilizacao { get; private set; }
        public DateTime? DataRevogacao { get; private set; }

        public int TentativasFalhas { get; private set; }
        public int MaximoTentativas { get; private set; }
        public bool EstaExpirado => DateTime.UtcNow >= DataExpiracao;
        public bool FoiUtilizado => DataUtilizacao.HasValue;
        public bool FoiRevogado => DataRevogacao.HasValue;
        public bool AtingiuLimiteTentativa => TentativasFalhas >= MaximoTentativas;
        public bool EstaAtivo =>
            !EstaExpirado && !FoiUtilizado && !FoiRevogado && !AtingiuLimiteTentativa;

        private CodigoConfirmacaoEmail() { }

        public CodigoConfirmacaoEmail(
            Guid usuarioId,
            string codigoHash,
            DateTime dataExpiracao,
            int maximoTentativas
        )
        {
            if (usuarioId == Guid.Empty)
            {
                throw new DomainException("O usuário do token de confirmação é obrigatório");
            }

            if (string.IsNullOrWhiteSpace(codigoHash))
            {
                throw new DomainException("O hash do token de confirmação é obrigatório");
            }

            if (dataExpiracao <= DateTime.UtcNow)
            {
                throw new DomainException(
                    "A data de expiração do token de confirmação deve ser futura"
                );
            }

            if (maximoTentativas <= 0)
            {
                throw new DomainException("O número máximo de tentativas deve ser maior que zero");
            }

            Id = Guid.NewGuid();
            UsuarioId = usuarioId;
            CodigoHash = codigoHash;
            DataCriacao = DateTime.UtcNow;
            DataExpiracao = dataExpiracao;
            MaximoTentativas = maximoTentativas;
        }

        public void RegistrarTentativaFalha()
        {
            if (!EstaAtivo)
            {
                throw new DomainException("O código de confirmação não está ativo");
            }

            TentativasFalhas++;
        }

        public void MarcarComoUtilizado()
        {
            if (!EstaAtivo)
            {
                throw new DomainException("O token de confirmação não está ativo");
            }

            DataUtilizacao = DateTime.UtcNow;
        }

        public void Revogar()
        {
            if (FoiRevogado)
            {
                throw new DomainException("O token de confirmação já foi revogado");
            }

            if (FoiUtilizado)
            {
                throw new DomainException("O token de confirmação já foi utilizado");
            }

            DataRevogacao = DateTime.UtcNow;
        }
    }
}
