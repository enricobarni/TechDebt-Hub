using TechDebtHub.Domain.Enums;

namespace TechDebtHub.Application.Features.DividasTecnicas.AlterarStatusDividaTecnica
{
    public sealed record AlterarStatusDividaTecnicaCommand(Guid Id, StatusDivida NovoStatus);
}
