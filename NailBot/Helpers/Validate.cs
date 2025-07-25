﻿using NailBot.Core.Entities;
using NailBot.Core.Exceptions;

namespace NailBot.Helpers
{
    //класс с методами валидации на превышение количества задач или на превышение количества символов в задаче
    public static class Validate
    {
        //метод валидации и парсинга числа
        internal static int ParseAndValidateInt(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("Введена строка из пробелов или пустая строка");

            bool parsingRelust = int.TryParse(str, out int res);

            if (parsingRelust)
                if (res > 0 && res < 100)
                    return res;

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
                {
                    return resultStr;
                }
                throw new TaskLengthLimitException(resultStr, strLength);
            }
            throw new ArgumentException("Введена строка из пробелов или пустая строка");
        }

        // метод валидации задачи
        internal static (string, Guid) ValidateTask(string input, Guid taskGuid, IReadOnlyList<ToDoItem> tasksList)
        {
            //сохраню исходный ввод пользака
            string startInput = input;

            if (input.StartsWith("/removetask "))
            {
                input = "/removetask";
            }                
            else
            {
                input = "/completetask";
            }

            if (tasksList.Count != 0)
            {
                if (!Guid.TryParse(startInput.Substring(input.Length), out taskGuid))
                {
                    throw new ArgumentException($"Введён некорректный номер задачи.\n");
                }

                if (tasksList.FirstOrDefault(x => x.Id == taskGuid) == null)
                {
                    throw new ArgumentException($"Введён некорректный номер задачи.\n");
                }
                return (input, taskGuid);
            }
            return (input, taskGuid);
        }
    }
}
