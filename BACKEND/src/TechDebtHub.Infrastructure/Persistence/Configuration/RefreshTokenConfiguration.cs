using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Infrastructure.Persistence.Configuration
{
    public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(refreshToken => refreshToken.Id);

            builder.Property(refreshToken => refreshToken.TokenHash).HasMaxLength(64).IsRequired();

            builder.Property(refreshToken => refreshToken.DataCriacao).IsRequired();

            builder.Property(refreshToken => refreshToken.DataExpiracao).IsRequired();

            builder.Property(refreshToken => refreshToken.DataRevogacao).IsRequired(false);

            builder
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(refreshToken => refreshToken.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(refreshToken => refreshToken.TokenHash).IsUnique();

            builder.HasIndex(refreshToken => refreshToken.UsuarioId);
        }
    }
}
