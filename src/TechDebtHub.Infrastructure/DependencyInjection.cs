using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Infrastructure.Persistence;

namespace TechDebtHub.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var connectionString = configuration.GetConnectionString("SqlServer");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "A connection string 'SqlServer' não foi encontrada nas configurações"
                );
            }

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString)
            );

            services.AddScoped<IApplicationDbContext>(serviceProvider =>
                serviceProvider.GetRequiredService<ApplicationDbContext>()
            );

            return services;
        }
    }
}
