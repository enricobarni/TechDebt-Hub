using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Exceptions
{
    public sealed class NotFoundException : Exception
    {
        public NotFoundException(string message)
            : base(message) { }
    }
}
