using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.Core.Exceptions
{
    public class DuplicateListException : Exception
    {
        public DuplicateListException(string list)
            : base($"Задача \"{list}\" является дубликатом, выберите другое имя")
        {
        }
    }
}
