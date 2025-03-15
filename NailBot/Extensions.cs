using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NailBot
{
    public static class Extensions
    {
        //метод рендера списка команд
        public static void CommandsRender<T>(this T array, bool echo, int echoNum) where T : Enum
        {
            int counter = 0;

            Console.WriteLine("Список доступных команд:");
            foreach (T command in Enum.GetValues(typeof(T)))
            {
                counter++;

                if (echo)
                    Console.WriteLine($"{counter}) /{command.ToString().ToLower()}");
                else
                {
                    if (counter != echoNum)
                    {
                        if (counter >= echoNum)
                            Console.WriteLine($"{counter - 1}) /{command.ToString().ToLower()}");
                        else
                            Console.WriteLine($"{counter}) /{command.ToString().ToLower()}");
                    }
                }
            }
            Console.WriteLine("");
        }

        //метод замены ввода номера команды
        public static string NumberReplacer(this string str)
        {
            Regex regex = new Regex("^[0-9]$");

            if (regex.IsMatch(str))
                str = "uncorrect command";
            return str;
        }

        public class StartValues()
        {
            //количество задач при запуске программы
            public int MaxTaskAmount { get; set; }

            //длина задачи при запуске программы
            public int MaxTaskLenght { get; set; }
           
            public StartValues(int maxTaskAmount, int maxTaskLenght) : this()
            {
                MaxTaskAmount = maxTaskAmount;
                MaxTaskLenght = maxTaskLenght;
            }
        }
        public static StartValues GetLengthValues(this int maxTaskAmount, int maxTaskLenght)
        {
            if (maxTaskAmount == 0)
            {
                //спрашиваем при запуске программы до тех пор пока не получим валидное значение
                Console.WriteLine("Введите максимально допустимое количество задач");

                //переопределение количества задач при успешном парсинге
                maxTaskAmount = Validate.ParseAndValidateInt(Console.ReadLine(), 1, 100);
            }

            if (maxTaskLenght == 0)
            {
                //спрашиваем при запуске программы до тех пор пока не получим валидное значение
                Console.WriteLine("Введите максимально допустимую длину задачи");

                //переопределение количества задач при успешном парсинге
                maxTaskLenght = Validate.ParseAndValidateInt(Console.ReadLine(), 1, 100);
            }

            return new StartValues(maxTaskAmount, maxTaskLenght);
        }
    }
}
