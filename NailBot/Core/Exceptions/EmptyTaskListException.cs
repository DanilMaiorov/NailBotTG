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
