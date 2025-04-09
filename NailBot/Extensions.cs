using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;
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
        public static void CommandsRender<T>(this T array, ToDoUser user, ITelegramBotClient botClient, Update update) where T : Enum
        {
            int counter = 0;

            if (user.UserId == Guid.Empty)
            {
                botClient.SendMessage(update.Message.Chat, $"Список доступных команд для незарегистрированного юзера:");
                foreach (T command in Enum.GetValues(typeof(T)))
                {
                    if (command.ToString() == "Start" || command.ToString() == "Help" || command.ToString() == "Info" || command.ToString() == "Exit")
                        Console.WriteLine($"{++counter}) /{command.ToString().ToLower()}");
                }
            } 
            else
            {
                botClient.SendMessage(update.Message.Chat, $"Список доступных команд:");

                foreach (T command in Enum.GetValues(typeof(T)))
                    Console.WriteLine($"{++counter}) /{command.ToString().ToLower()}");
                
                Console.WriteLine("");
            }
        }

        //метод замены ввода номера команды
        public static string NumberReplacer(this string str)
        {
            Regex regex = new Regex("^[0-9]$");

            if (regex.IsMatch(str))
                str = "uncorrect command";
            return str;
        }


        //метод присваивания значений длин
        public static int GetStartValues(this int value, string str, ITelegramBotClient botClient, Update update)
        {
            while (value == 0)
            {
                //спрашиваем при запуске программы до тех пор пока не получим валидное значение
                botClient.SendMessage(update.Message.Chat, str);
                return value = Validate.ParseAndValidateInt(Console.ReadLine(), 1, 100);
            }
            return value;
        }


    }
}
