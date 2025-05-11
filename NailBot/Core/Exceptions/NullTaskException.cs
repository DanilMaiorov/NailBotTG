using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.Core.Exceptions
{
    internal class NullTaskException : Exception
    {
        public NullTaskException(string message)
            : base(message)
        {
        }
    }
}
