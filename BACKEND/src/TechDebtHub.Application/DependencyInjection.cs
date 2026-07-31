using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TechDebtHub.Application.Features.Projetos.AtualizarProjeto;
using TechDebtHub.Application.Features.Projetos.BuscarPorId;
using TechDebtHub.Application.Features.Projetos.CriarProjeto;
using TechDebtHub.Application.Features.Projetos.ListarProjetos;

namespace TechDebtHub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CriarProjetoHandler>();
            services.AddScoped<BuscarPorIdHandler>();
            services.AddScoped<ListarProjetosHandler>();
            services.AddScoped<AtualizarProjetoHandler>();

            return services;
        }
    }
}
