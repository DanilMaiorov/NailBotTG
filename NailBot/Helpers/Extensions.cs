using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;
using System.Text.RegularExpressions;
using NailBot.Core.Entities;

namespace NailBot.Helpers
{
    public static class Extensions
    {
        //метод рендера списка команд
        public static void CommandsRender<T>(this T array, ToDoUser user, Chat chat, ITelegramBotClient botClient) where T : Enum
        {
            int counter = 0;

            if (user == null)
            {
                botClient.SendMessage(chat, $"Список доступных команд для незарегистрированного юзера:");
                foreach (T command in Enum.GetValues(typeof(T)))
                {
                    if (command.ToString() == "Start" || command.ToString() == "Help" || command.ToString() == "Info" || command.ToString() == "Exit")
                    {
                        Console.WriteLine($"{++counter}) /{command.ToString().ToLower()}");
                    }
                }
            }
            else
            {
                botClient.SendMessage(chat, $"Список доступных команд:");

                foreach (T command in Enum.GetValues(typeof(T)))
                {
                    Console.WriteLine($"{++counter}) /{command.ToString().ToLower()}");
                }
                Console.WriteLine("");
            }
        }

        //метод замены ввода номера команды
        public static string NumberReplacer(this string str)
        {
            Regex regex = new Regex("^[0-9]$");

            if (regex.IsMatch(str))
            {
                str = "uncorrect command";
            }
                
            return str;
        }

        //метод присваивания значений длин
        public static int GetStartValues(this int value, string message)
        {
            while (value == 0)
            {
                while (true)
                {
                    try
                    {
                        Console.WriteLine(message);
                        return value = Validate.ParseAndValidateInt(Console.ReadLine());
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return value;
        }
    }
}

