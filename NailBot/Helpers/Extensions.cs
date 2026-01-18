using System.Text;
using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Domain;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailBot.Helpers
{
    public static class Extensions
    {
        //метод рендера списка команд
        public static string CommandsRender()
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
                { Commands.AddTask, "Добавить новую задачу" },
                { Commands.Show, "Показать активные задачи" },
                { Commands.Find, "Найти задачу" },
                { Commands.Report, "Сформировать отчет по задачам" },
                { Commands.Exit, "Выйти" }
            };

            builder.AppendLine("Список доступных команд:");

            foreach (Commands commandValue in Enum.GetValues(typeof(Commands)))    
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

            return builder.ToString();

            //await botClient.SetMyCommands(commands, cancellationToken: ct);
        }
        
        
        
        public static MessageData? MessageGetData(Update update, CancellationToken ct)
        {
            if (update.Message != null)
                return new MessageData(
                    update.Message.Chat,
                    update.Message.Text?.Trim(),
                    update.Message.Id,
                    update.Message.From.Id
                );
            if (update.CallbackQuery != null)
                return new MessageData(
                    update.CallbackQuery.Message.Chat,
                    update.CallbackQuery.Data?.Trim(),
                    update.CallbackQuery.Message.Id,
                    update.CallbackQuery.From.Id
                );
            return null;
        }
    }
    
    

}

