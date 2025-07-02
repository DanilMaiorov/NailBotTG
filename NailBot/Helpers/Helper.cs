using NailBot.Core.Entities;
using NailBot.Core.Exceptions;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.Json;
using NailBot.Core.Enums;

namespace NailBot.Helpers
{

    public static class Helper
    {
        //инициализирую клавиатуры
        //кнопка для /start
        public static readonly ReplyKeyboardMarkup keyboardStart = new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton("/start")
            })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        //кнопки для зареганных юзеров
        public static readonly ReplyKeyboardMarkup keyboardReg = new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton("ФВЫВАВЫА"),
                new KeyboardButton("/showtasks"),
                new KeyboardButton("/report")
            })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };
        //рендер списка задач
        public async static Task TasksListRender(IReadOnlyList<ToDoItem> tasks, ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            int taskCounter = 0;

            var builder = new StringBuilder();

            foreach (ToDoItem task in tasks)
            {
                taskCounter++;
                await botClient.SendMessage(chat, $"{taskCounter}) ({task.State}) {task.Name} - {task.CreatedAt}", cancellationToken: ct);
                await botClient.SendMessage(chat, $"```Id\n{task.Id}```", parseMode: ParseMode.MarkdownV2, cancellationToken: ct);
            }
        }

        //метод проверки корректного ввода команд /addtask, /removetask, /completetask, /find
        public static (string, string, Guid) InputCheck(string input, IReadOnlyList<ToDoItem> currentUserTaskList = null)
        {
            string cutInput = "";
            Guid taskGuid = Guid.Empty;

            if (input.StartsWith("/addtask") || input.StartsWith("/removetask") || input.StartsWith("/completetask") || input.StartsWith("/find"))
            {
                if (input.StartsWith("/addtask "))
                {
                    cutInput = input.Substring(9);
                    input = "/addtask";
                }
                else if (input.StartsWith("/find "))
                {
                    cutInput = input.Substring(6);
                    input = "/find";
                }
                else if (input.StartsWith("/removetask ") || input.StartsWith("/completetask "))
                {
                    //верну данные кортежем
                    (string command, Guid taskGuid) inputData = Validate.ValidateTask(input, taskGuid, currentUserTaskList);

                    input = inputData.command;
                    taskGuid = inputData.taskGuid;
                }
                else
                {
                    input = "unregistered user command";
                }
            }
            return (input, cutInput, taskGuid);
        }

        //проверка дубликатов
        public static void CheckDuplicate(string newTask, IReadOnlyList<ToDoItem> toDoItems)
        {
            if (toDoItems.Any(item => item.Name == newTask))
            {
                throw new DuplicateTaskException(newTask);
            }
        }

        //метод присваивания значений длин
        public static int GetStartValues(string message)
        {
            int value = 0;
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

        /// <summary>
        /// Преобразует строковое представление в значение указанного перечисления.
        /// </summary>
        /// <typeparam name="T">Тип перечисления</typeparam>
        /// <param name="input">Входная строка для преобразования</param>
        /// <returns>
        /// Соответствующее значение перечисления при успешном преобразовании.
        /// Значение по умолчанию для типа перечисления при неудаче.
        /// </returns>
        /// <remarks>
        /// Сравнение выполняется без учёта регистра. 
        /// Если входная строка не соответствует ни одному элементу перечисления, возвращается default(T).
        /// </remarks>
        public static T GetEnumValue<T>(string input) where T : struct, Enum
        {
            return Enum.TryParse<T>(input, true, out var result)
                ? result
                : default;
        }
    }
}
