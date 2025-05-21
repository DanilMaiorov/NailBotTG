using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;
using System.Text.RegularExpressions;
using NailBot.Core.Entities;

namespace NailBot.Helpers
{
    public static class Extensions
    {
        //метод рендера списка команд
        public async static Task CommandsRender<T>(this T array, ToDoUser user, Chat chat, ITelegramBotClient botClient, CancellationToken ct) where T : Enum
        {
            int counter = 0;

            if (user == null)
            {
                await botClient.SendMessage(chat, $"Список доступных команд для незарегистрированного юзера:", ct);
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
                await botClient.SendMessage(chat, $"Список доступных команд:", ct);

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
                
            return regex.IsMatch(str) ? "uncorrect command" : str;
        }
    }
}

