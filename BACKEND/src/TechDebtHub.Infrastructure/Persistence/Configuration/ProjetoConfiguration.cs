using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Infrastructure.Persistence.Configuration
{
    public sealed class ProjetoConfiguration : IEntityTypeConfiguration<Projeto>
    {
        public void Configure(EntityTypeBuilder<Projeto> builder)
        {
            builder.ToTable("Projetos");

            builder.HasKey(projeto => projeto.Id);

            builder.Property(projeto => projeto.Nome).HasMaxLength(120).IsRequired();

            builder.Property(projeto => projeto.NomeNormalizado).HasMaxLength(120).IsRequired();

            builder.HasIndex(projeto => projeto.NomeNormalizado).IsUnique();

            builder.Property(projeto => projeto.Descricao).HasMaxLength(1000).IsRequired();

            builder.Property(projeto => projeto.Arquivado).IsRequired();
        }
    }
}
