using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Infrastructure.Persistence.Configuration
{
    public sealed class DividaTecnicaConfiguration : IEntityTypeConfiguration<DividaTecnica>
    {
        public void Configure(EntityTypeBuilder<DividaTecnica> builder)
        {
            builder.ToTable("DividasTecnicas");

            builder.HasKey(dividas => dividas.Id);

            builder.Property(divida => divida.Titulo).HasMaxLength(120).IsRequired();

            builder.Property(divida => divida.TituloNormalizado).HasMaxLength(120).IsRequired();

            builder
                .HasIndex(divida => new { divida.ProjetoId, divida.TituloNormalizado })
                .IsUnique();

            builder.Property(divida => divida.Descricao).HasMaxLength(2000).IsRequired();

            builder.Property(divida => divida.Categoria).IsRequired();

            builder.Property(divida => divida.Status).IsRequired();

            builder.Property(divida => divida.Impacto).IsRequired();

            builder.Property(divida => divida.Urgencia).IsRequired();

            builder.Property(divida => divida.Frequencia).IsRequired();

            builder.Property(divida => divida.Esforco).IsRequired();

            builder.Property(divida => divida.PontuacaoPrioridade).HasPrecision(10, 2).IsRequired();

            builder
                .HasOne<Projeto>()
                .WithMany()
                .HasForeignKey(divida => divida.ProjetoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
