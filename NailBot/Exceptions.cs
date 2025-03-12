using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NailBot
{
    //исключение на превышение количества задач
    public class TaskCountLimitException : Exception
    {
        public int TaskLimit { get; }

        public TaskCountLimitException(string message, int taskCountLimit)
            : base(message) 
        {
            TaskLimit = taskCountLimit;
        }


    }
}
