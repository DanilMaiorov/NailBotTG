using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NailBot
{

    //класс с методами валидации
    public static class Validate
    {
        internal static int ParseAndValidateInt(string? str, int min, int max)
        {
            bool parsingRelust = int.TryParse(str, out int limit);

            if (!parsingRelust)
                throw new ArgumentException();

            if (limit < 1 || limit > 100)
                throw new ArgumentException();

            return limit;
            
        }
    }
    //Добавить метод int ParseAndValidateInt(string? str, int min, int max),
    //который приводит полученную строку к int и проверяет, что оно находится в диапазоне min и max.
    //В противном случае выбрасывать ArgumentException с сообщением. Добавить использование этого метода в приложение.
    //исключение на превышение количества задач
}
