using System.Net;
using TechDebtHub.Api.IntegrationTests.Common;

namespace TechDebtHub.Api.IntegrationTests;

public sealed class ProjetosControllerTests : ApiIntegrationTestBase
{
    public ProjetosControllerTests(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task Get_Projetos_SemFiltros_DeveRetornar200ComListaVazia()
    {
        var response = await Client.GetAsync("/projetos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
