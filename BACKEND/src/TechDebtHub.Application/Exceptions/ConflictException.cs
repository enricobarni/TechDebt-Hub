using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechDebtHub.Application.Exceptions
{
    public sealed class ConflictException : Exception
    {
        public ConflictException(string message)
            : base(message) { }
    }
}
