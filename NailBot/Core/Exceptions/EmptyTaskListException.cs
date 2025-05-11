using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.Core.Exceptions
{
    internal class EmptyTaskListException : Exception
    {
        public EmptyTaskListException(string message)
            : base($"Ваш список задач пуст, {message} нечего.\n")
        {
        }
    }
}
