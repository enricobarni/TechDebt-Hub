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
        : IEntityTypeConfiguration<CodigoConfirmacaoEmail>
    {
        public void Configure(EntityTypeBuilder<CodigoConfirmacaoEmail> builder)
        {
            builder.ToTable("TokensConfirmaçãoEmail");

            builder.HasKey(codigo => codigo.Id);

            builder.Property(codigo => codigo.CodigoHash).HasMaxLength(64).IsRequired();

            builder.Property(codigo => codigo.DataCriacao).IsRequired();

            builder.Property(codigo => codigo.DataExpiracao).IsRequired();

            builder.Property(codigo => codigo.DataUtilizacao).IsRequired(false);

            builder.Property(codigo => codigo.DataRevogacao).IsRequired(false);

            builder.Property(codigo => codigo.TentativasFalhas).IsRequired();

            builder.Property(codigo => codigo.MaximoTentativas).IsRequired();

            builder
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(codigo => codigo.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(codigo => new { codigo.UsuarioId, codigo.CodigoHash });

            builder.Ignore(codigo => codigo.EstaExpirado);
            builder.Ignore(codigo => codigo.FoiUtilizado);
            builder.Ignore(codigo => codigo.FoiRevogado);
            builder.Ignore(codigo => codigo.AtingiuLimiteTentativa);
            builder.Ignore(codigo => codigo.EstaAtivo);
        }
    }
}
