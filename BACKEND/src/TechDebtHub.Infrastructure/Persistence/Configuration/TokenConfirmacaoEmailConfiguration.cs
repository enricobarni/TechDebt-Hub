using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Infrastructure.Persistence.Configuration
{
    public sealed class TokenConfirmacaoEmailConfiguration
        : IEntityTypeConfiguration<TokenConfirmacaoEmail>
    {
        public void Configure(EntityTypeBuilder<TokenConfirmacaoEmail> builder)
        {
            builder.ToTable("TokensConfirmaçãoEmail");

            builder.HasKey(token => token.Id);

            builder.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();

            builder.Property(token => token.DataCriacao).IsRequired();

            builder.Property(token => token.DataExpiracao).IsRequired();

            builder.Property(token => token.DataUtilizacao).IsRequired(false);

            builder.Property(token => token.DataRevogacao).IsRequired(false);

            builder
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(token => token.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(token => token.UsuarioId);

            builder.Ignore(token => token.EstaExpirado);
            builder.Ignore(token => token.FoiUtilizado);
            builder.Ignore(token => token.FoiRevogado);
            builder.Ignore(token => token.EstaAtivo);
        }
    }
}
