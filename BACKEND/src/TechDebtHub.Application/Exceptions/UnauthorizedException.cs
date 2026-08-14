using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Exceptions
{
    public sealed class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message)
            : base(message) { }
    }
}
