using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Exceptions;

namespace TechDebtHub.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string TokenHash { get; private set; } = string.Empty;
        public DateTime DataCriacao { get; private set; }
        public DateTime DataExpiracao { get; private set; }
        public DateTime? DataRevogacao { get; private set; }
        public bool EstaExpirado => DateTime.UtcNow >= DataExpiracao;
        public bool FoiRevogado => DataRevogacao.HasValue;
        public bool EstaAtivo => !EstaExpirado && !FoiRevogado;

        private RefreshToken() { }

        public RefreshToken(Guid usuarioId, string tokenHash, DateTime dataExpiracao)
        {
            if (usuarioId == Guid.Empty)
            {
                throw new DomainException("O usuário do refresh token é obrigatório");
            }

            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                throw new DomainException("O hash do refresh token é obrigatório");
            }

            if (dataExpiracao <= DateTime.UtcNow)
            {
                throw new DomainException("A data de expiração do refresh token deve ser futura");
            }

            Id = Guid.NewGuid();
            UsuarioId = usuarioId;
            TokenHash = tokenHash;
            DataCriacao = DateTime.UtcNow;
            DataExpiracao = dataExpiracao;
        }

        public void Revogar()
        {
            if (FoiRevogado)
            {
                throw new DomainException("O refresh token já foi revogado");
            }

            DataRevogacao = DateTime.UtcNow;
        }
    }
}
