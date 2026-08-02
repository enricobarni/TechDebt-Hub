using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.AlterarStatusDividaTecnica
{
    public sealed record AlterarStatusDividaTecnicaResponse(
        Guid Id,
        StatusDivida Status,
        DateTime? DataAtualizacao,
        DateTime? DataResolucao
    );
}