using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Infrastructure.Persistence.Configuration
{
    public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(usuario => usuario.Id);

            builder.Property(usuario => usuario.Nome).HasMaxLength(120).IsRequired();

            builder.Property(usuario => usuario.Email).HasMaxLength(254).IsRequired();

            builder.Property(usuario => usuario.EmailNormalizado).HasMaxLength(254).IsRequired();

            builder.Property(usuario => usuario.SenhaHash).HasMaxLength(500).IsRequired();

            builder.Property(usuario => usuario.Ativo).IsRequired().HasDefaultValue(true);

            builder
                .Property(usuario => usuario.EmailConfirmado)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(usuario => usuario.DataCriacao).IsRequired();

            builder.Property(usuario => usuario.DataAtualizacao).IsRequired(false);

            builder.Property(usuario => usuario.DataConfirmacaoEmail).IsRequired(false);

            builder.HasIndex(usuario => usuario.EmailNormalizado).IsUnique();
        }
    }
}
