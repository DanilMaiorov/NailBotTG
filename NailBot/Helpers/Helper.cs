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
using System.Reflection;
using NailBot.TelegramBot.Scenarios;

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
        public static InlineKeyboardMarkup GetSelectListKeyboardForShow(IReadOnlyList<ToDoList> lists)
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
            ListInlineButtonGenerate(lists, keyboardRows, "show");

            //последний ряд кнопок
            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "🆕 Добавить", callbackData: "addlist"),
                InlineKeyboardButton.WithCallbackData(text: "❌ Удалить", callbackData: "deletelist")
            });

            return new InlineKeyboardMarkup(keyboardRows);
        }

        //метод клавиатуры после нажатия /show
        public static InlineKeyboardMarkup GetSelectListKeyboardForDelete(IReadOnlyList<ToDoList> lists)
        {
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();

            // кнопки списков
            ListInlineButtonGenerate(lists, keyboardRows, "deletelist");

            return new InlineKeyboardMarkup(keyboardRows);
        }

        //метод клавиатуры выбора списка куда добавлять задачу
        public static InlineKeyboardMarkup GetSelectListKeyboardForAdd(IReadOnlyList<ToDoList> lists)
        {
            //первый ряд
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();

            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    text: "📌 Без списка",
                    callbackData: new ToDoListCallbackDto { Action = "add", ToDoListId = null }.ToString()
                )
            });

            // кнопки списков
            ListInlineButtonGenerate(lists, keyboardRows, "add");

            return new InlineKeyboardMarkup(keyboardRows);
        }

        //метод клавиатуры подтверждения удаления списка
        public static InlineKeyboardMarkup GetApproveDeleteListKeyboard()
        {
            //первый ряд
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();

            //последний ряд кнопок
            keyboardRows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(text: "✅ Да", callbackData: "yes"),
                InlineKeyboardButton.WithCallbackData(text: "❌ Нет", callbackData: "no")
            });

            return new InlineKeyboardMarkup(keyboardRows);
        }

        //рендер списка задач
        public async static Task TasksListRender(IReadOnlyList<ToDoItem> tasks, ITelegramBotClient botClient, Chat chat, CancellationToken ct)
        {
            int taskCounter = 0;

            var builder = new StringBuilder();

            foreach (ToDoItem task in tasks)
            {
                if (task.State == ToDoItemState.Active)
                {
                    taskCounter++;
                    await botClient.SendMessage(chat, $"{taskCounter}) ({task.State}) {task.Name} - {task.CreatedAt}", cancellationToken: ct);
                    await botClient.SendMessage(chat, $"```Id\n{task.Id}```", parseMode: ParseMode.MarkdownV2, cancellationToken: ct);
                }
            }
        }
        //добавлю перегрузку TasksListRender
        public async static Task TasksListRender(IReadOnlyList<ToDoItem> tasks, ITelegramBotClient botClient, Chat chat, bool isActive, CancellationToken ct)
        {
            int taskCounter = 0;

            var builder = new StringBuilder();

            foreach (ToDoItem task in tasks)
            {
                if (isActive)
                {
                    taskCounter++;
                    await botClient.SendMessage(chat, $"{taskCounter}) ({task.State}) {task.Name} - {task.CreatedAt}", cancellationToken: ct);
                    await botClient.SendMessage(chat, $"```Id\n{task.Id}```", parseMode: ParseMode.MarkdownV2, cancellationToken: ct);
                }
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
                    input = "unregistered user command";
                
            }
            return (input, cutInput, taskGuid);
        }

        //проверка дубликатов
        public static void CheckDuplicate(string newTask, IReadOnlyList<ToDoItem> toDoItems)
        {
            if (toDoItems.Any(item => item.Name == newTask))
                throw new DuplicateTaskException(newTask);
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

        /// <summary>
        /// Собирает путь до директории из переданных папок. При отутствии директории - создает директорию
        /// </summary>
        /// <param name="args">Названия папок до директории для построения пути</param>
        /// <returns>Путь к директории</returns>
        public static string GetDirectoryPath(params string[] args)
        {
            string path = args.Aggregate(Path.Combine);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        /// <summary>
        /// Собирает путь до директории из переданных папок. При отустствии директории - создает директорию
        /// </summary>
        /// <param name="args">Названия папок до директории для построения пути</param>
        public static void CheckOrCreateDirectory(params string[] args)
        {
            string path = args.Aggregate(Path.Combine);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Сохраняет JSON-файл по указанному пути
        /// </summary>
        /// <param name="item">Задача пользователя</param>
        /// <param name="ct">Токен отмены</param>
        /// <param name="args">Названия папок до директории для построения пути</param>
        public static async void CreateToDoItemJsonFile(ToDoItem item, CancellationToken ct, params string[] args)
        {
            var fileDirectory = GetDirectoryPath(args);

            var json = JsonSerializer.Serialize(item);

            var filePath = Path.Combine(fileDirectory, $"{item.Id}.json");

            await File.WriteAllTextAsync(filePath, json, ct);
        }

        /// <summary>
        /// Парсит строку формата "префикс|GUID", разделённую символом '|', и возвращает GUID из второй части.
        /// Возвращает null, если строка пустая, не содержит разделителя или GUID невалиден.
        /// </summary>
        /// <param name="input">Входная строка для парсинга</param>
        /// <param name="commandPrefix">Ожидаемый префикс перед разделителем</param>
        /// <returns>GUID из строки или null</returns>
        public static Guid? ParseGuidFromCommand(string input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            string[] parts = input.Split('|');

            if (parts.Length > 1 && Guid.TryParse(parts[1], out Guid result))
                return result;
            
            return null;
        }

        /// <summary>
        /// Фабричный метод для создания контекста сценария
        /// </summary>
        /// <param name="type">Тип создаваемого сценария</param>
        /// <returns>Новый экземпляр ScenarioContext</returns>
        public static ScenarioContext CreateScenarioContext(ScenarioType type)
        {
            return new ScenarioContext(type);
        }

        //написать метод генерации списка
        private static void ListInlineButtonGenerate(IReadOnlyList<ToDoList> lists, List<IEnumerable<InlineKeyboardButton>> keyboardRows, string action)
        {
            foreach (var list in lists)
            {
                keyboardRows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: list.Name,
                        callbackData: new ToDoListCallbackDto { Action = action, ToDoListId = list.Id }.ToString()
                    )
                });
            }
        }

    }
}
