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

            bool parsingRelust = int.TryParse(str, out int limit);

            if (parsingRelust)
                if (limit > 0 && limit < 100)
                    return limit;

            throw new Exception("Ошибка ввода, ожидаемый ввод: число от 1 до 100");

            //тут я написал ещё 1 try catch чтобы в Main можно было прокидывать просто через throw с полным StackTrace и во внешнем catch обращаться к InnerException

            //try
            //{
            //    bool parsingRelust = int.TryParse(str, out int limit);

            //    if (parsingRelust)
            //    {
            //        if (limit > 1 && limit < 100)
            //            return limit;
            //    }
            //    throw new ArgumentException("Ошибка ввода, ожидаемый ввод: число от 1 до 100");
            //}
            //catch (ArgumentException ex)
            //{
            //    throw new ArgumentException("Произошла непредвиденная ошибка", ex);
            //}
        }

        //метод валидации строки
        internal static string ValidateString(string? str, out string resultStr)
        {
            if (!string.IsNullOrWhiteSpace(str))
                return resultStr = str.Trim();

            throw new ArgumentException("Введена строка из пробелов или пустая строка");
        }
    }
}
