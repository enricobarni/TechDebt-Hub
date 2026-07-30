using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Api.Middleware;

namespace TechDebtHub.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApi(this IServiceCollection services)
        {
            services.AddScoped<ExceptionHandlingMiddleware>();
            return services;
        }
    }
}