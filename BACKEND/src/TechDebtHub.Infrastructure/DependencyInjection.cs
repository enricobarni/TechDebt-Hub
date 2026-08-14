using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens.Experimental;
using TechDebtHub.Application.Abstractions.Persistence;
using TechDebtHub.Application.Interfaces;
using TechDebtHub.Infrastructure.Persistence;
using TechDebtHub.Infrastructure.Security;

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

            services
                .AddOptions<JwtSettings>()
                .Bind(configuration.GetSection(JwtSettings.SectionName))
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.Issuer),
                    "Jwt:Issuer não foi configurado"
                )
                .Validate(
                    settings => !string.IsNullOrWhiteSpace(settings.Audience),
                    "Jwt:Audience não foi configurado"
                )
                .Validate(
                    settings => IsValidSigningKey(settings.SigningKey),
                    "Jwt:SigningKey deve ser uma chave Base64 válida com pelo menos 32 bytes"
                )
                .Validate(
                    settings => settings.ExpirationMinutes > 0,
                    "Jwt:ExpirationMinutes deve ser maior que zero"
                )
                .ValidateOnStart();

            services.AddSingleton<IPasswordHasher, Argon2idPasswordHasher>();
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }

        private static bool IsValidSigningKey(string signingKey)
        {
            if (string.IsNullOrWhiteSpace(signingKey))
            {
                return false;
            }

            try
            {
                var keyBytes = Convert.FromBase64String(signingKey);

                return keyBytes.Length >= 32;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
