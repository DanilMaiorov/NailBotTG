using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NailBot
{
    //класс с методами валидации на превышение количества задач или на превышение количества символов в задаче
    public static class Validate
    {
        //метод валидации и парсинга числа
        internal static int ParseAndValidateInt(string? str, int min, int max)
        {
            //выбрасываю ошибки и отображаю их во внешнем catch в Main как InnerException

            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Введена строка из пробелов или пустая строка");

            bool parsingRelust = int.TryParse(str, out int limit);

            if (parsingRelust)
                if (limit > 0 && limit < 100)
                    return limit;

            throw new ArgumentException("Ошибка ввода, ожидаемый ввод: число от 1 до 100");

        }

        //метод валидации строки
        internal static string ValidateString(string? str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                string resultStr;
                return resultStr = str.Trim();
            }
            throw new ArgumentException("Введена строка из пробелов или пустая строка");
        }

        //добавлю перегрузку метода валидации строки
        internal static string ValidateString(string? str, int strLength)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                string resultStr = str.Trim();

                if (resultStr.Length < strLength)
                    return resultStr;
                throw new TaskLengthLimitException(resultStr, strLength);
            }
            throw new ArgumentException("Введена строка из пробелов или пустая строка");
        }

        // метод валидации задачи
        internal static (string, string, Guid) ValidateTask(string input, string cutInput, Guid taskGuid, List<ToDoItem> tasksList)
        {
            //сохраню исходный ввод пользака
            string startInput = input;


            if (input.StartsWith("/removetask "))
                input = "/removetask";
            else
                input = "/completetask";

            //перенес проверку на длину списка задач сюда
            if (tasksList.Count != 0)
            {
                cutInput = startInput.Substring(input.Length);

                bool success = int.TryParse(cutInput, out int taskNumber);

                if (success && (taskNumber > 0 && taskNumber <= tasksList.Count))
                {
                    taskGuid = tasksList[taskNumber - 1].Id;
                    return (input, cutInput, taskGuid);
                }
                else
                    throw new ArgumentException($"Введён некорректный номер задачи.\n");
                }
            return (input, cutInput, taskGuid);
        }
    }
}
