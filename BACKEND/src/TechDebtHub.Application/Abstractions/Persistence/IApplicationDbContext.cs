using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Application.Abstractions.Persistence
{
    public interface IApplicationDbContext
    {
        DbSet<Projeto> Projetos { get; }

        DbSet<DividaTecnica> DividasTecnicas { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
