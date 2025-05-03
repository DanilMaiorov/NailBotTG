using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NailBot.Core.Exceptions
{
    //исключение на дубликат задачи
    public class DuplicateTaskException : Exception
    {
        public DuplicateTaskException(string task)
            : base($"Задача \"{task}\" является дубликатом, она не будет добавлена")
        {
        }
    }
}
