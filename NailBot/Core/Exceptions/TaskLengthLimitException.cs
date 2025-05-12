namespace NailBot.Core.Exceptions
{
    //исключение на превышение длины задачи
    public class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(string task, int taskLengthLimit)
            : base($"Длина задачи \"{task}\" - {task.Length}, что превышает максимально допустимое значение {taskLengthLimit}")
        {
        }
    }
}
