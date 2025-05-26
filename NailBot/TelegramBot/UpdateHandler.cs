using NailBot.Core.Entities;
using NailBot.Core.Services;
using NailBot.Helpers;
using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NailBot.TelegramBot;
public enum Commands
{
    Start = 1, 
    Help, 
    Info, 
    Addtask, 
    Showtasks, 
    Showalltasks, 
    Removetask, 
    Find, 
    Completetask, 
    Report, 
    Exit
}


internal delegate void MessageEventHandler(string message);

internal class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoReportService _toDoReportService;

    //добавлю токен
    private readonly CancellationToken _ct;

    //добавлю 2 события
    public event MessageEventHandler OnHandleUpdateStarted;
    public event MessageEventHandler OnHandleUpdateCompleted;

    public UpdateHandler(IUserService iuserService, IToDoService itoDoService, IToDoReportService itoDoReportService, CancellationToken ct)
    {
        _userService = iuserService ?? throw new ArgumentNullException(nameof(iuserService));
        _toDoService = itoDoService ?? throw new ArgumentNullException(nameof(itoDoService));

        _toDoReportService = itoDoReportService ?? throw new ArgumentNullException(nameof(itoDoReportService));

        _ct = ct;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        var currentChat = update.Message.Chat;
        string message = "";


        //инициализирую клавиатуры
        //кнопка для /start
        ReplyKeyboardMarkup keyboardStart = new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton("/start")
            })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        //кнопки для зареганных юзеров
        ReplyKeyboardMarkup keyboardReg = new ReplyKeyboardMarkup(
            new[]
            {
                new KeyboardButton("/showalltasks"),
                new KeyboardButton("/showtasks"),
                new KeyboardButton("/report")
            })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };

        try
        {
            var currentUser = await _userService.GetUser(update.Message.From.Id, ct);

            var currentUserTaskList = currentUser != null
                ? await _toDoService.GetAllByUserId(currentUser.UserId, ct)
                : null;

            if (update.Message.Id == 1)
            {
                await botClient.SendMessage(currentChat, $"Привет! Это Todo List Bot! \n", cancellationToken: ct);
                return;
            }

            if (currentUser == null)
            {
                if (update.Message.Text != "/start")
                {
                    await botClient.SendMessage(currentChat, "До регистрации доступна только команда /start. Нажмите на кнопку ниже или введите /start", replyMarkup: keyboardStart, cancellationToken: ct);
                    return;
                }
            } 

            string input = update.Message.Text;

            //присваиваю начальное значение введёного сообщения
            message = input;

            //НАЧАЛО ОБРАБОТКИ СООБЩЕНИЯ
            OnHandleUpdateStarted?.Invoke(message);

            Commands command;

            input = input.NumberReplacer();

            (string inputCommand, string inputText, Guid taskGuid) = Helper.InputCheck(input, currentUserTaskList);

            input = inputCommand.Replace("/", string.Empty);

            if (currentUser == null && input != "start")
            {
                input = "unregistered user command";
            }

            if (Enum.TryParse<Commands>(input, true, out var result))
            {
                command = result;
            }
            else
            {
                command = default;
            }

            //КОНЕЦ ОБРАБОТКИ СООБЩЕНИЯ
            OnHandleUpdateCompleted?.Invoke(message);
            switch (command)
            {
                case Commands.Start:
                    if (currentUser == null)
                    {
                        currentUser = await _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username, ct);
                    }
                    await botClient.SendMessage(currentChat, "Спасибо за регистрацию", replyMarkup: keyboardReg, cancellationToken: ct);
                    await Commands.Start.CommandsRender(currentUser, currentChat, botClient, ct);
                    break;

                case Commands.Help:
                    await ShowHelp(currentUser);
                    break;

                case Commands.Info:
                    await ShowInfo();
                    break;

                case Commands.Addtask:
                    var newTask = await _toDoService.Add(currentUser, inputText, ct);
                    await botClient.SendMessage(currentChat, $"Задача \"{newTask.Name}\" добавлена в список задач.\n", replyMarkup: keyboardReg, cancellationToken: ct);
                    break;

                case Commands.Showtasks:
                    await ShowTasks(currentUser.UserId, keyboardReg);
                    break;

                case Commands.Showalltasks:
                    await ShowTasks(currentUser.UserId, keyboardReg, true);
                    break;

                case Commands.Removetask:
                    //вызов метода удаления задачи
                    await _toDoService.Delete(taskGuid, ct);
                    await botClient.SendMessage(currentChat, $"Задача {taskGuid} удалена.\n", replyMarkup: keyboardReg, cancellationToken: ct);
                    break;

                case Commands.Completetask:
                    await _toDoService.MarkCompleted(taskGuid, ct);
                    await botClient.SendMessage(currentChat, $"Задача {taskGuid} выполнена.\n", replyMarkup: keyboardReg, cancellationToken: ct);
                    break;

                case Commands.Find:
                    var findedTasks = await _toDoService.Find(currentUser, inputText, ct);
                    await ShowTasks(currentUser.UserId, keyboardReg, true, findedTasks);
                    break;

                case Commands.Report:
                    var (total, completed, active, generatedAt) = await _toDoReportService.GetUserStats(currentUser.UserId, ct);
                    await botClient.SendMessage(currentChat, $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};", replyMarkup: keyboardReg, cancellationToken: ct);
                    break;

                case Commands.Exit:
                    await botClient.SendMessage(currentChat, "Нажмите CTRL+C (Ввод) для остановки бота", replyMarkup: keyboardReg, cancellationToken: ct);
                    break;
                default:
                    await botClient.SendMessage(currentChat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n", replyMarkup: keyboardReg, cancellationToken: ct);
                    await Commands.Start.CommandsRender(currentUser, currentChat, botClient, ct);
                    break;
            }
        }
        #region КАСТОМНЫЕ ИСКЛЮЧЕНИЯ
        //catch (ArgumentException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, cancellationToken: ct);

        //    if (update.Message.Id == 1)
        //        await HandleUpdateAsync(botClient, update, ct);
        //}
        //catch (TaskCountLimitException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, cancellationToken: ct);

        //    if (update.Message.Id == 1)
        //        await HandleUpdateAsync(botClient, update, ct);
        //}
        //catch (TaskLengthLimitException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, cancellationToken: ct);

        //    if (update.Message.Id == 1)
        //        await HandleUpdateAsync(botClient, update, ct);
        //}
        //catch (DuplicateTaskException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, cancellationToken: ct);
        //}
        //catch (EmptyTaskListException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, cancellationToken: ct);
        //}
        #endregion

        catch (Exception)
        {
            //await botClient.SendMessage(currentChat, $"Произошла непредвиденная ошибка", cancellationToken: ct);
            throw;
        }
        #region МЕТОДЫ КОМАНД
        async Task ShowTasks(Guid userId, ReplyKeyboardMarkup keyboard, bool isActive = false, IReadOnlyList<ToDoItem>? tasks = null)
        {
            //присвою список через оператор null объединения 
            var tasksList = tasks ?? (isActive 
                ? await _toDoService.GetAllByUserId(userId, ct) 
                : await _toDoService.GetActiveByUserId(userId, ct));

            if (tasksList.Count == 0) 
            {
                string emptyMessage = isActive ? "Список задач пуст.\n" : "Aктивных задач нет";
                await botClient.SendMessage(currentChat, emptyMessage, replyMarkup: keyboardReg, cancellationToken: ct);
                return;
            }

            //выберу текст меседжа через тернарный оператор
            string message = tasks != null ? "Список найденных задач:"
                : (isActive ? "Список всех задач:" : "Список активных задач:");

            await botClient.SendMessage(currentChat, message, replyMarkup: keyboardReg, cancellationToken: ct);

            await Helper.TasksListRender(tasksList, botClient, currentChat, ct);
        }

        async Task ShowHelp(ToDoUser user)
        {
            if (user == null)
            {
                await botClient.SendMessage(currentChat, $"Незнакомец, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n", replyMarkup: keyboardReg, cancellationToken: ct);
            }
            else
            {
                await botClient.SendMessage(currentChat, $"{user.TelegramUserName}, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/addtask\" *название задачи*\" ты сможешь добавлять задачи в список задач\n" +
                $"Введя команду \"/showtasks\" ты сможешь увидеть список активных задач в списке\n" +
                $"Введя команду \"/showalltasks\" ты сможешь увидеть список всех задач в списке\n" +
                $"Введя команду \"/removetask\" *номер задачи*\" ты сможешь удалить задачу из списка задач\n" +
                $"Введя команду \"/completetask\" *номер задачи*\" ты сможешь отметить задачу из списка как завершенную\n" +
                $"Введя команду \"/find\" *название задачи*\" ты сможешь увидеть список всех задач начинающихся с названия задачи\n" +
                $"Введя команду \"/report\" ты получишь отчёт по задачам\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n", replyMarkup: keyboardReg, cancellationToken: ct);
            }
        }

        async Task ShowInfo()
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            await botClient.SendMessage(currentChat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n", replyMarkup: keyboardReg, cancellationToken: ct);
        }
        #endregion
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        Console.WriteLine($"Обработанное исключение: {exception.Message}");

        return Task.CompletedTask;
    }

    public void HandleStart(string message)
    {
        Console.WriteLine($"Началась обработка сообщения \"{message}\"\n");
    }

    public void HandleComplete(string message)
    {
        Console.WriteLine($"Закончилась обработка сообщения \"{message}\"\n");
    }
}