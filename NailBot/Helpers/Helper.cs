using NailBot.Core.Entities;
using NailBot.Core.Exceptions;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.Json;
using NailBot.Core.Enums;
using System.Globalization;
using NailBot.TelegramBot.Dto;

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
                new KeyboardButton("/show"),
                new KeyboardButton("/addtask"),
                new KeyboardButton("/report")
            })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };
        //кнопка для /cancel
        public static readonly ReplyKeyboardMarkup keyboardCancel = new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton("/cancel")
            })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };



        //метод клавиатуры после нажатия /show
        public static InlineKeyboardMarkup GetSelectListKeyboardWithAdd(IReadOnlyList<ToDoList> lists)
        {
            //первый ряд
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();

            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: "📌 Без списка",
                    callbackData: new ToDoListCallbackDto { Action = "show", ToDoListId = null }.ToString()
                )
            });

            // кнопки списков
            foreach (var list in lists)
            {
                keyboardRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: list.Name,
                        callbackData: new ToDoListCallbackDto { Action = "show", ToDoListId = list.Id }.ToString()
                    )
                });
            }

            //последний ряд кнопок
            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "🆕 Добавить", callbackData: "addlist"),
                InlineKeyboardButton.WithCallbackData(text: "❌ Удалить", callbackData: "deletelist")
            });

            return new InlineKeyboardMarkup(keyboardRows);
        }

        //метод клавиатуры выбора списка куда добавлять задачу
        public static InlineKeyboardMarkup GetSelectListKeyboard(IReadOnlyList<ToDoList> lists)
        {
            //первый ряд
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();

            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: "📌 Без списка",
                    callbackData: new ToDoListCallbackDto { Action = "show", ToDoListId = null }.ToString()
                )
            });

            // кнопки списков
            foreach (var list in lists)
            {
                keyboardRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: list.Name,
                        callbackData: new ToDoListCallbackDto { Action = "show", ToDoListId = list.Id }.ToString()
                    )
                });
            }

            return new InlineKeyboardMarkup(keyboardRows);
        }

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

            if (input.StartsWith("/removetask") || input.StartsWith("/completetask") || input.StartsWith("/find"))
            {
                if (input.StartsWith("/find "))
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

            if (input == "add_list")
            {
                
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

        /// <summary>
        /// Пытается преобразовать строку с датой в формате dd.MM.yyyy в объект DateTime.
        /// </summary>
        /// <param name="userInput">Строка с датой для парсинга</param>
        /// <param name="result">Результат преобразования (при успешном парсинге)</param>
        /// <returns>
        /// true - если парсинг выполнен успешно; 
        /// false - если строка имеет неверный формат или не является допустимой датой
        /// </returns>
        /// <remarks>
        /// Использует строгий парсинг с форматом, заданным в Constants.deadlineFormat
        /// и независимый от региональных настроек (InvariantCulture)
        /// </remarks>
        public static bool TryParseUserDate(string userInput, out DateTime result)
        {
            return DateTime.TryParseExact(
                userInput,
                Constants.deadlineFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out result
            );
        }




        public static string GetDirectoryPath(params string[] args)
        {
            string path = args.Aggregate(Path.Combine);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }


        public static void CheckOrCreateDirectory(params string[] args)
        {
            string path = args.Aggregate(Path.Combine);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }


    }
}
