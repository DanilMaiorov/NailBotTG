namespace NailBot.Core.Exceptions
{
    //исключение на превышение количества задач
    public class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int taskCountLimit)
            : base($"Превышено максимальное количество задач равное {taskCountLimit}")
        {
        }
    }
}
