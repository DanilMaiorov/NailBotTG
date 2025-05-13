using NailBot.Core.Entities;
using NailBot.Core.Services;
using NailBot.Helpers;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace NailBot.TelegramBot;
public enum Commands
{
    Start = 1, Help, Info, Addtask, Showtasks, Showalltasks, Removetask, Find, Completetask, Report, Exit
}

internal delegate void MessageEventHandler(string message);

internal class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoReportService _toDoReportService;

    private readonly ToDoService toDoService;
    private readonly UserService userService;

    //добавлю токен сорс
    private readonly CancellationTokenSource _cts;

    //добавлю 2 события
    public event MessageEventHandler OnHandleUpdateStarted;
    public event MessageEventHandler OnHandleUpdateCompleted;

    public UpdateHandler(IUserService iuserService, IToDoService itoDoService, IToDoReportService itoDoReportService, CancellationTokenSource cts)
    {
        _userService = iuserService ?? throw new ArgumentNullException(nameof(iuserService));
        _toDoService = itoDoService ?? throw new ArgumentNullException(nameof(itoDoService));

        _toDoReportService = itoDoReportService ?? throw new ArgumentNullException(nameof(itoDoReportService));

        _cts = cts;

        //явно приведу к типу
        toDoService = (ToDoService)itoDoService;
        userService = (UserService)iuserService;
    }


    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        var currentChat = update.Message.Chat;
        string message = "";
        try
        {
            var currentUser = _userService.GetUser(update.Message.From.Id);

            var currentUserTaskList = currentUser != null
                ? _toDoService.GetAllByUserId(currentUser.UserId)
                : null;

            if (update.Message.Id == 1)
            {
                await botClient.SendMessage(currentChat, $"Привет! Это Todo List Bot! Введите команду для начала работы или выхода из бота.\n", ct);

                await Commands.Start.CommandsRender(currentUser, currentChat, botClient, ct);

                return;
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



            if (currentUser == null)
            {
                if (input != "start" && input != "help" && input != "info" && input != "exit")
                {
                    input = "unregistered user command";
                }
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
                        currentUser = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
                    }

                    await Commands.Start.CommandsRender(currentUser, currentChat, botClient, ct);
                    break;

                case Commands.Help:
                    await ShowHelp(currentUser);
                    break;

                case Commands.Info:
                    await ShowInfo();
                    break;

                case Commands.Addtask:
                    var newTask = _toDoService.Add(currentUser, inputText);
                    await botClient.SendMessage(currentChat, $"Задача \"{newTask.Name}\" добавлена в список задач.\n", ct);
                    break;

                case Commands.Showtasks:
                    await ShowTasks(currentUser.UserId);
                    break;

                case Commands.Showalltasks:
                    await ShowTasks(currentUser.UserId, true);
                    break;

                case Commands.Removetask:
                    //вызов метода удаления задачи
                    _toDoService.Delete(taskGuid);
                    await botClient.SendMessage(currentChat, $"Задача {taskGuid} удалена.\n", ct);
                    break;

                case Commands.Completetask:
                    _toDoService.MarkCompleted(taskGuid);
                    await botClient.SendMessage(currentChat, $"Задача {taskGuid} выполнена.\n", ct);
                    break;

                case Commands.Find:
                    var findedTasks = _toDoService.Find(currentUser, inputText);
                    await ShowTasks(currentUser.UserId, false, findedTasks);
                    break;

                case Commands.Report:
                    var (total, completed, active, generatedAt) = _toDoReportService.GetUserStats(currentUser.UserId);
                    await botClient.SendMessage(currentChat, $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};", ct);
                    break;

                case Commands.Exit:
                    await botClient.SendMessage(currentChat, "Нажмите любую клавишу для остановки бота", ct);
                    _cts.Cancel();
                    //throw new OperationCanceledException(ct);
                    break;
                default:
                    await botClient.SendMessage(currentChat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n", ct);
                    await Commands.Start.CommandsRender(currentUser, currentChat, botClient, ct);
                    break;
            }
        }
        #region КАСТОМНЫЕ ИСКЛЮЧЕНИЯ
        //catch (ArgumentException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, ct);

        //    if (update.Message.Id == 1)
        //        await HandleUpdateAsync(botClient, update, ct);
        //}
        //catch (TaskCountLimitException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, ct);

        //    if (update.Message.Id == 1)
        //        await HandleUpdateAsync(botClient, update, ct);
        //}
        //catch (TaskLengthLimitException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, ct);

        //    if (update.Message.Id == 1)
        //        await HandleUpdateAsync(botClient, update, ct);
        //}
        //catch (DuplicateTaskException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, ct);
        //}
        //catch (EmptyTaskListException ex)
        //{
        //    await botClient.SendMessage(currentChat, ex.Message, ct);
        //}
        #endregion

        catch (Exception)
        {
            //await botClient.SendMessage(currentChat, $"Произошла непредвиденная ошибка", ct);
            throw;
        }
        #region МЕТОДЫ КОМАНД
        async Task ShowTasks(Guid userId, bool isActive = false, IReadOnlyList<ToDoItem>? tasks = null)
        {
            //присвою список через оператор null объединения 
            var tasksList = tasks ?? (isActive 
                ? _toDoService.GetAllByUserId(userId) 
                : _toDoService.GetActiveByUserId(userId));

            if (tasksList.Count == 0) 
            {
                string emptyMessage = isActive ? "Список задач пуст.\n" : "Aктивных задач нет";
                await botClient.SendMessage(currentChat, emptyMessage, ct);
                return;
            }

            //выберу текст меседжа через тернарный оператор
            string message = tasks != null ? "Список найденных задач:"
                : (isActive ? "Список всех задач:" : "Список активных задач:");

            await botClient.SendMessage(currentChat, message, ct);

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
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n", ct);
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
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n", ct);
            }
        }

        async Task ShowInfo()
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            await botClient.SendMessage(currentChat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n", ct);
        }
        #endregion


    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
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