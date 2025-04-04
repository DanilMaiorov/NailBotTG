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
    }
}
