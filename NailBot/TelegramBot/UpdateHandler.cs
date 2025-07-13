using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using NailBot.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Channels;
using NailBot.Core.Exceptions;
using Telegram.Bots.Http;
using Telegram.Bots;
using NailBot.TelegramBot.Dto;
using System.Reflection;

namespace NailBot.TelegramBot;

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

    //логика сценариев
    private readonly IEnumerable<IScenario> _scenarios;
    private readonly IScenarioContextRepository _scenarioContextRepository;

    //IToDoListService 
    private readonly IToDoListService _toDoListService;

    public UpdateHandler(
        IUserService iuserService, 
        IToDoService itoDoService, 
        IToDoReportService itoDoReportService, 
        IEnumerable<IScenario> scenarios, 
        IScenarioContextRepository contextRepository,
        IToDoListService itoDoListService,
        CancellationToken ct)
    {
        _userService = iuserService ?? throw new ArgumentNullException(nameof(iuserService));
        _toDoService = itoDoService ?? throw new ArgumentNullException(nameof(itoDoService));

        _toDoReportService = itoDoReportService ?? throw new ArgumentNullException(nameof(itoDoReportService));

        _scenarios = scenarios;
        _scenarioContextRepository = contextRepository;

        _toDoListService = itoDoListService;

        _ct = ct;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        Chat currentChat;
        long telegramUserId;
        string input;
        string message = "";

        if (update.Message != null)
        {
            currentChat = update.Message.Chat;
            telegramUserId = update.Message.From.Id;
            input = update.Message.Text;

            await OnMessage(botClient, update, update.Message, ct);
        }
        else if (update.CallbackQuery != null)
        {
            currentChat = update.CallbackQuery.Message.Chat;
            telegramUserId = update.CallbackQuery.From.Id;
            input = update.CallbackQuery.Data;

            await OnCallbackQuery(botClient, update, update.CallbackQuery, ct);
        }
        else
        {
            await OnUnknown(update);
            return;
        }
    }


    private async Task OnMessage(ITelegramBotClient botClient, Update update, Message message, CancellationToken ct)
    {
        Chat currentChat = update.Message.Chat;
        long telegramUserId = update.Message.From.Id;
        string input = update.Message.Text;

        try
        {
            var currentUser = await _userService.GetUser(telegramUserId, ct);

            var currentUserTaskList = currentUser != null
                ? await _toDoService.GetAllByUserId(currentUser.UserId, ct)
                : null;

            //if (update.Message.Id == 1)
            //{
            //    await botClient.SendMessage(currentChat, $"Привет! Это Todo List Bot! \n", cancellationToken: ct);
            //    return;
            //}

            if (currentUser == null)
            {
                if (input != "/start")
                {
                    await botClient.SendMessage(currentChat, "До регистрации доступна только команда /start. Нажмите на кнопку ниже или введите /start", replyMarkup: Helper.keyboardStart, cancellationToken: ct);

                    return;
                }
            }

            //НАЧАЛО ОБРАБОТКИ СООБЩЕНИЯ
            OnHandleUpdateStarted?.Invoke(message.Text);

            (string inputCommand, string inputText, Guid taskGuid) = Helper.InputCheck(input, currentUserTaskList);

            input = inputCommand.Replace("/", string.Empty);

            if (currentUser == null && input != "start")
                input = "unregistered user command";

            //получение значений команд типа Enum
            Commands command = Helper.GetEnumValue<Commands>(input);
            ScenarioType scenarioType = Helper.GetEnumValue<ScenarioType>(input);

            //КОНЕЦ ОБРАБОТКИ СООБЩЕНИЯ
            OnHandleUpdateCompleted?.Invoke(message.Text);

            //Работа с командой cancel и сценариями
            if (command == Commands.Cancel)
                await _scenarioContextRepository.ResetContext(telegramUserId, ct);


            var scenarioContext = await _scenarioContextRepository.GetContext(telegramUserId, ct);

            if (scenarioContext != null)
            {
                await ProcessScenario(scenarioContext, update, ct);
                return;
            }


            switch (command)
            {
                case Commands.Start:
                    if (currentUser == null)
                        currentUser = await _userService.RegisterUser(telegramUserId, update.Message.From.Username, ct);

                    await botClient.SendMessage(currentChat, "Спасибо за регистрацию", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    await Commands.Start.CommandsRender(currentUser, currentChat, botClient, ct);
                    break;

                case Commands.Help:
                    await ShowHelp(currentUser);
                    break;

                case Commands.Info:
                    await ShowInfo();
                    break;

                case Commands.Addtask:
                    var newContext = new ScenarioContext(ScenarioType.AddTask);
                    await ProcessScenario(newContext, update, ct);
                    break;

                case Commands.Cancel:
                    await botClient.SendMessage(currentChat, "Текущий сценарий отменён. Выбирай что хочешь сделать?", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
                case Commands.Show:

                    var lists = await _toDoListService.GetUserLists(currentUser.UserId, ct);
                    await botClient.SendMessage(currentChat, "Выберите список", replyMarkup: Helper.GetSelectListKeyboardForShow(lists), cancellationToken: ct);
                    break;

                case Commands.Removetask:
                    await _toDoService.Delete(taskGuid, ct);
                    await botClient.SendMessage(currentChat, $"Задача {taskGuid} удалена.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;

                case Commands.Completetask:
                    await _toDoService.MarkCompleted(taskGuid, ct);
                    await botClient.SendMessage(currentChat, $"Задача {taskGuid} выполнена.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;

                case Commands.Find:
                    var findedTasks = await _toDoService.Find(currentUser, inputText, ct);
                    await ShowTasks(currentUser.UserId, true, findedTasks);
                    break;

                case Commands.Report:
                    var (total, completed, active, generatedAt) = await _toDoReportService.GetUserStats(currentUser.UserId, ct);
                    await botClient.SendMessage(currentChat, $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;

                case Commands.Exit:
                    await botClient.SendMessage(currentChat, "Нажмите CTRL+C (Ввод) для остановки бота", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
                default:
                    await botClient.SendMessage(currentChat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
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
        catch (TaskCountLimitException ex)
        {
            await _scenarioContextRepository.ResetContext(telegramUserId, ct);
            await botClient.SendMessage(currentChat, "Текущий сценарий завершен. Нужно почистить список задач.", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        }
        //catch (TaskLengthLimitException ex)
        //{
        //    await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
        //    await botClient.SendMessage(currentChat, "Нужно почистить список задач.", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        //}
        catch (DuplicateTaskException ex)
        {
            await botClient.SendMessage(currentChat, ex.Message, cancellationToken: ct);
            await botClient.SendMessage(currentChat, "Введите название задачи заново или нажмите кнопку отмены:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
            return;
        }
        catch (EmptyTaskListException ex)
        {
            await botClient.SendMessage(currentChat, ex.Message, cancellationToken: ct);
            await botClient.SendMessage(currentChat, "Введите название задачи заново или нажмите кнопку отмены:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
        }
        #endregion

        catch (Exception)
        {
            //await botClient.SendMessage(currentChat, $"Произошла непредвиденная ошибка", cancellationToken: ct);
            throw;
        }
        #region МЕТОДЫ КОМАНД
        async Task ShowTasks(Guid userId, bool isActive = false, IReadOnlyList<ToDoItem>? tasks = null)
        {
            //присвою список через оператор null объединения 
            var tasksList = tasks ?? (isActive
                ? await _toDoService.GetAllByUserId(userId, ct)
                : await _toDoService.GetActiveByUserId(userId, ct));

            if (tasksList.Count == 0)
            {
                string emptyMessage = isActive ? "Список задач пуст\n" : "Aктивных задач нет";
                await botClient.SendMessage(currentChat, emptyMessage, replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                return;
            }

            //выберу текст меседжа через тернарный оператор
            string message = tasks != null ? "Список найденных задач:"
                : (isActive ? "Список всех задач:" : "Список активных задач:");

            await botClient.SendMessage(currentChat, message, replyMarkup: Helper.keyboardReg, cancellationToken: ct);

            if (isActive)
                await Helper.TasksListRender(tasksList, botClient, currentChat, isActive, ct);
            else
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
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
            }
            else
            {
                await botClient.SendMessage(currentChat, $"{user.TelegramUserName}, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/addtask\" будет предложено ввести название задачи и при успешном вводе, задача будет добавлена\n" +
                $"Введя команду \"/cancel\" ты сможешь отменить отменить добавление новой задачи \n" +
                $"Введя команду \"/showtasks\" ты сможешь увидеть список активных задач в списке\n" +
                $"Введя команду \"/showalltasks\" ты сможешь увидеть список всех задач в списке\n" +
                $"Введя команду \"/removetask\" *номер задачи*\" ты сможешь удалить задачу из списка задач\n" +
                $"Введя команду \"/completetask\" *номер задачи*\" ты сможешь отметить задачу из списка как завершенную\n" +
                $"Введя команду \"/find\" *название задачи*\" ты сможешь увидеть список всех задач начинающихся с названия задачи\n" +
                $"Введя команду \"/report\" ты получишь отчёт по задачам\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
            }
        }

        async Task ShowInfo()
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            await botClient.SendMessage(currentChat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        }
        #endregion

        #region МЕТОДЫ СЦЕНАРИЯ
        /// <summary>
        /// Возвращает экземпляр сценария по указанному типу.
        /// </summary>
        /// <param name="scenario">Тип сценария из перечисления ScenarioType</param>
        /// <returns>Реализация интерфейса IScenario для запрошенного сценария</returns>
        /// <exception cref="NotSupportedException">Выбрасывается при передаче неподдерживаемого значения ScenarioType</exception>
        IScenario GetScenario(ScenarioType scenario)
        {
            var currentScenario = _scenarios.FirstOrDefault(s => s.CanHandle(scenario));
            return currentScenario ?? throw new NotSupportedException($"Сценарий {scenario} не поддерживается");
        }

        async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);

            var scenarioResult = await scenario.HandleMessageAsync(botClient, context, update, ct);

            if (scenarioResult == ScenarioResult.Completed)
                await _scenarioContextRepository.ResetContext(telegramUserId, ct);
            else
                await _scenarioContextRepository.SetContext(telegramUserId, context, ct);
        }
        #endregion

    }
    private async Task OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery, CancellationToken ct)
    {
        Chat currentChat = callbackQuery.Message.Chat;
        long telegramUserId = callbackQuery.From.Id;
        string input = callbackQuery.Data;

        try
        {
            var currentUser = await _userService.GetUser(telegramUserId, ct);

            //пока непонятно надо это или нет
            var currentUserTaskList = currentUser != null
                ? await _toDoService.GetAllByUserId(currentUser.UserId, ct)
                : null;

            if (currentUser == null)
            {
                if (input != "/start")
                {
                    await botClient.SendMessage(currentChat, "До регистрации доступна только команда /start. Нажмите на кнопку ниже или введите /start", replyMarkup: Helper.keyboardStart, cancellationToken: ct);

                    return;
                }
            }

            var callbackDto = CallbackDto.FromString(input);

            var toDoListCallbackDto = ToDoListCallbackDto.FromString(input);


            //НАЧАЛО ОБРАБОТКИ СООБЩЕНИЯ
            //OnHandleUpdateStarted?.Invoke(message.Text);

            //(string inputCommand, string inputText, Guid taskGuid) = Helper.InputCheck(input, currentUserTaskList);

            //получение значений команд типа Enum
            Commands command = Helper.GetEnumValue<Commands>(input);
            ScenarioType scenarioType = Helper.GetEnumValue<ScenarioType>(input);

            //КОНЕЦ ОБРАБОТКИ СООБЩЕНИЯ
            //OnHandleUpdateCompleted?.Invoke(message.Text);

            //Работа с командой cancel и сценариями
            if (command == Commands.Cancel)
                await _scenarioContextRepository.ResetContext(telegramUserId, ct);

            var scenarioContext = await _scenarioContextRepository.GetContext(telegramUserId, ct);

            var listGuid = toDoListCallbackDto.ToDoListId;

            ToDoList? list = null;

            if (scenarioContext != null)
            {
                if (scenarioContext.Data.TryGetValue(currentChat.Username, out object? item))
                {
                    if (item is ToDoItem toDoItem)
                    {
                        if (listGuid.HasValue)
                        {
                            toDoItem.List = await _toDoListService.Get(listGuid.Value, ct);
                        }
                    }
                }

                await ProcessScenario(scenarioContext, update, ct);

                return;
            }

                        

            switch (toDoListCallbackDto.Action)
            {
                case "show":
                    var toDoItems = await _toDoService.GetByUserIdAndList(currentUser.UserId, toDoListCallbackDto.ToDoListId, ct);

                    await ShowTasksFromFolder(toDoItems);

                    break;

                case "addlist":
                    var newContext = new ScenarioContext(ScenarioType.AddList);
                    await ProcessScenario(newContext, update, ct);
                    break;


                default:
                    await botClient.SendMessage(currentChat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
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
        catch (TaskCountLimitException ex)
        {
            await _scenarioContextRepository.ResetContext(telegramUserId, ct);
            await botClient.SendMessage(currentChat, "Текущий сценарий завершен. Нужно почистить список задач.", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        }
        //catch (TaskLengthLimitException ex)
        //{
        //    await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
        //    await botClient.SendMessage(currentChat, "Нужно почистить список задач.", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        //}
        catch (DuplicateTaskException ex)
        {
            await botClient.SendMessage(currentChat, ex.Message, cancellationToken: ct);
            await botClient.SendMessage(currentChat, "Введите название задачи заново или нажмите кнопку отмены:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
            return;
        }
        catch (EmptyTaskListException ex)
        {
            await botClient.SendMessage(currentChat, ex.Message, cancellationToken: ct);
            await botClient.SendMessage(currentChat, "Введите название задачи заново или нажмите кнопку отмены:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
        }
        #endregion

        catch (Exception)
        {
            //await botClient.SendMessage(currentChat, $"Произошла непредвиденная ошибка", cancellationToken: ct);
            throw;
        }
        #region МЕТОДЫ КОМАНД

        async Task ShowTasksFromFolder(IReadOnlyList<ToDoItem> tasksList, bool isActive = false)
        {
            if (tasksList.Count == 0)
            {
                string emptyMessage = isActive ? "Список задач пуст\n" : "Aктивных задач нет";
                await botClient.SendMessage(currentChat, emptyMessage, replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                return;
            }

            //выберу текст меседжа через тернарный оператор
            string message = isActive ? "Список всех задач:" : "Список активных задач:";

            await botClient.SendMessage(currentChat, message, replyMarkup: Helper.keyboardReg, cancellationToken: ct);

            await Helper.TasksListRender(tasksList, botClient, currentChat, ct);
        }



        async Task ShowTasks(Guid userId, bool isActive = false, IReadOnlyList<ToDoItem>? tasks = null)
        {
            //присвою список через оператор null объединения 
            var tasksList = tasks ?? (isActive
                ? await _toDoService.GetAllByUserId(userId, ct)
                : await _toDoService.GetActiveByUserId(userId, ct));

            if (tasksList.Count == 0)
            {
                string emptyMessage = isActive ? "Список задач пуст\n" : "Aктивных задач нет";
                await botClient.SendMessage(currentChat, emptyMessage, replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                return;
            }

            //выберу текст меседжа через тернарный оператор
            string message = tasks != null ? "Список найденных задач:"
                : (isActive ? "Список всех задач:" : "Список активных задач:");

            await botClient.SendMessage(currentChat, message, replyMarkup: Helper.keyboardReg, cancellationToken: ct);

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
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
            }
            else
            {
                await botClient.SendMessage(currentChat, $"{user.TelegramUserName}, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/addtask\" будет предложено ввести название задачи и при успешном вводе, задача будет добавлена\n" +
                $"Введя команду \"/cancel\" ты сможешь отменить отменить добавление новой задачи \n" +
                $"Введя команду \"/showtasks\" ты сможешь увидеть список активных задач в списке\n" +
                $"Введя команду \"/showalltasks\" ты сможешь увидеть список всех задач в списке\n" +
                $"Введя команду \"/removetask\" *номер задачи*\" ты сможешь удалить задачу из списка задач\n" +
                $"Введя команду \"/completetask\" *номер задачи*\" ты сможешь отметить задачу из списка как завершенную\n" +
                $"Введя команду \"/find\" *название задачи*\" ты сможешь увидеть список всех задач начинающихся с названия задачи\n" +
                $"Введя команду \"/report\" ты получишь отчёт по задачам\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
            }
        }

        async Task ShowInfo()
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            await botClient.SendMessage(currentChat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        }
        #endregion

        #region МЕТОДЫ СЦЕНАРИЯ
        /// <summary>
        /// Возвращает экземпляр сценария по указанному типу.
        /// </summary>
        /// <param name="scenario">Тип сценария из перечисления ScenarioType</param>
        /// <returns>Реализация интерфейса IScenario для запрошенного сценария</returns>
        /// <exception cref="NotSupportedException">Выбрасывается при передаче неподдерживаемого значения ScenarioType</exception>
        IScenario GetScenario(ScenarioType scenario)
        {
            var currentScenario = _scenarios.FirstOrDefault(s => s.CanHandle(scenario));
            return currentScenario ?? throw new NotSupportedException($"Сценарий {scenario} не поддерживается");
        }

        async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);

            var scenarioResult = await scenario.HandleMessageAsync(botClient, context, update, ct);

            if (scenarioResult == ScenarioResult.Completed)
                await _scenarioContextRepository.ResetContext(telegramUserId, ct);
            else
                await _scenarioContextRepository.SetContext(telegramUserId, context, ct);
        }
        #endregion


    }

    private async Task OnUnknown(Update update)
    {
        Console.WriteLine($"Unknown update type: {update.Type}");
        await Task.Delay(1);
        // Возможно, отправить сообщение пользователю о неизвестной команде
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