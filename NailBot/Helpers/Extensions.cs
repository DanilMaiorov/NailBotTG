using System.Text;
using System.Text.RegularExpressions;
using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.TelegramBot;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailBot.Helpers
{
    public static class Extensions
    {
        //метод рендера списка команд
        public async static Task CommandsRender<T>(this T array, ToDoUser user, Chat chat, ITelegramBotClient botClient, CancellationToken ct) where T : Enum
        {
            int counter = 0;
            //создам стрингБилдер для сборки в одно сообщение, а не пачки
            var builder = new StringBuilder();

            //команды бота
            var commands = new List<BotCommand>();

            //заведу словарик для описания команд
            var commandDescriptions = new Dictionary<Commands, string>()
            {
                { Commands.Start, "Начало работы с ботом, регистрация" },
                { Commands.Help, "Помощь по командам" },
                { Commands.Info, "Информация о боте" },
                { Commands.Addtask, "Добавить новую задачу" },
                { Commands.Showtasks, "Показать активные задачи" },
                { Commands.Showalltasks, "Показать все задачи" },
                { Commands.Removetask, "Удалить задачу" },
                { Commands.Find, "Найти задачу" },
                { Commands.Completetask, "Отметить задачу как выполненную" },
                { Commands.Report, "Сформировать отчет по задачам" },
                { Commands.Exit, "Выйти" }
            };

            builder.AppendLine("Список доступных команд:");

            foreach (T commandValue in Enum.GetValues(typeof(T)))    
            {
                Commands command = (Commands)Enum.Parse(typeof(Commands), commandValue.ToString());

                string commandName = $"/{command.ToString().ToLower()}";

                if (commandDescriptions.TryGetValue(command, out string? description))
                {
                    builder.AppendLine($"{++counter}) {commandName} - {description}");
                    commands.Add(new BotCommand { Command = commandName, Description = description });
                }
                else
                {
                    builder.AppendLine($"{++counter}) {commandName}");
                    commands.Add(new BotCommand { Command = commandName, Description = "" });
                }
            }

            await botClient.SendMessage(chat, builder.ToString(), cancellationToken: ct);

            //рендерю менюшку
            await botClient.SetMyCommands(commands, cancellationToken: ct);
        }

        //метод замены ввода номера команды
        public static string NumberReplacer(this string str)
        {
            Regex regex = new Regex("^[0-9]$");

            return regex.IsMatch(str) ? "uncorrect command" : str;
        }
    }
}

