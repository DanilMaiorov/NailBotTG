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
