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

    //исключение на превышение длины задачи
    public class TaskLengthLimitException : Exception
    {
        public int TaskLengthLimit { get; }
        public int TaskLength { get; }

        public TaskLengthLimitException(string message, int taskLengthLimit, int taskLength)
            : base(message)
        {
            TaskLengthLimit = taskLengthLimit;
            TaskLength = taskLength;
        }
    }

    //исключение на дубликат задачи
    public class DuplicateTaskException : Exception
    {
        public string Task { get; }

        public DuplicateTaskException(string message, string task)
            : base(message)
        {
            Task = task;
        }
    }
}
