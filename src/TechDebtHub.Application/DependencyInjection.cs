using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TechDebtHub.Application.Features.Projetos.BuscarPorId;
using TechDebtHub.Application.Features.Projetos.CriarProjeto;

namespace TechDebtHub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CriarProjetoHandler>();
            services.AddScoped<BuscarPorIdHandler>();

            return services;
        }
    }
}
