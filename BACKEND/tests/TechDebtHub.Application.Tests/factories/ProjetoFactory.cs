using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Domain.Entities;

namespace TechDebtHub.Application.Tests.Factories
{
    public static class ProjetoFactory
    {
        public static Projeto Criar(
            string nome = "TechDebt Hub",
            string descrição = "Sistema para controle de dívidas técnicas"
        )
        {
            return Projeto.Criar(nome, descrição);
        }
    }
}
