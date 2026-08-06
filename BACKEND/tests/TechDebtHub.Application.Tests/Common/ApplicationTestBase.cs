using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Infrastructure.Persistence;

namespace TechDebtHub.Application.Tests.Common
{
    public abstract class ApplicationTestBase : IAsyncLifetime
    {
        private TestDbContextFactory? _factory;

        protected ApplicationDbContext Context =>
            _factory?.Context
            ?? throw new InvalidOperationException("O banco de testes ainda não foi inicializado");

        public async Task InitializeAsync()
        {
            _factory = await TestDbContextFactory.CriarAsync();
        }

        public async Task DisposeAsync()
        {
            if (_factory is not null)
            {
                await _factory.DisposeAsync();
            }
        }
    }
}
