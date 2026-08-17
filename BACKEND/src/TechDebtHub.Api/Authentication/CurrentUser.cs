using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechDebtHub.Application.Interfaces;

namespace TechDebtHub.Api.Authentication
{
    public sealed class CurrentUser : ICurrentUser
    {
        private const string SubjectClaim = "sub";
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UsuarioId
        {
            get
            {
                var subject = _httpContextAccessor.HttpContext?.User.FindFirst(SubjectClaim)?.Value;

                return Guid.TryParse(subject, out var usuarioId) ? usuarioId : null;
            }
        }
    }
}
