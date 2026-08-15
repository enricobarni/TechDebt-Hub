using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TechDebtHub.Application.Features.DividasTecnicas.AlterarStatusDividaTecnica;
using TechDebtHub.Application.Features.DividasTecnicas.ArquivarDividaTecnica;
using TechDebtHub.Application.Features.DividasTecnicas.AtualizarDividaTecnica;
using TechDebtHub.Application.Features.DividasTecnicas.BuscarDividaTecnicaPorId;
using TechDebtHub.Application.Features.DividasTecnicas.CriarDividaTecnica;
using TechDebtHub.Application.Features.DividasTecnicas.ListarDividasTecnicas;
using TechDebtHub.Application.Features.Projetos.ArquivarProjeto;
using TechDebtHub.Application.Features.Projetos.AtualizarProjeto;
using TechDebtHub.Application.Features.Projetos.BuscarPorId;
using TechDebtHub.Application.Features.Projetos.CriarProjeto;
using TechDebtHub.Application.Features.Projetos.ListarProjetos;
using TechDebtHub.Application.Features.Usuarios.CadastrarUsuario;
using TechDebtHub.Application.Features.Usuarios.Login;
using TechDebtHub.Application.Features.Usuarios.RefreshTokens;

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
            services.AddScoped<ArquivarProjetoHandler>();
            services.AddScoped<CriarDividaTecnicaHandler>();
            services.AddScoped<BuscarDividaTecnicaPorIdHandler>();
            services.AddScoped<ListarDividasTecnicasHandler>();
            services.AddScoped<AtualizarDividaTecnicaHandler>();
            services.AddScoped<AlterarStatusDividaTecnicaHandler>();
            services.AddScoped<ArquivarDividaTecnicaHandler>();
            services.AddScoped<CadastrarUsuarioHandler>();
            services.AddScoped<LoginHandler>();
            services.AddScoped<RefreshTokensHandler>();

            return services;
        }
    }
}
