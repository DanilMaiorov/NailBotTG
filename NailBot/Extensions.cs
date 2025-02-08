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
                        if (counter == echoNum + 1)
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
            Regex regex = new Regex("^[0-5]$");

            if (regex.IsMatch(str))
                str = "uncorrect command";
            return str;
        }
    }
}
