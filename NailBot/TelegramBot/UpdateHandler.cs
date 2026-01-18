using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using NailBot.TelegramBot.Scenarios;
using NailBot.Core.Exceptions;
using NailBot.TelegramBot.Dto;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace NailBot.TelegramBot;

internal delegate void MessageEventHandler(string message);

internal class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoReportService _toDoReportService;

    //добавлю 2 события
    public event MessageEventHandler? OnHandleUpdateStarted;
    public event MessageEventHandler? OnHandleUpdateCompleted;

    //логика сценариев
    private readonly IEnumerable<IScenario> _scenarios;
    private readonly IScenarioContextRepository _scenarioContextRepository;

    //IToDoListService 
    private readonly IToDoListService _toDoListService;
    
    //количество кнопок задач на 1 странице
    int _pageSize = 5;

    public UpdateHandler(
        IUserService userService, //
        IToDoService toDoService, //
        IToDoReportService toDoReportService, //
        IEnumerable<IScenario> scenarios, 
        IScenarioContextRepository contextRepository,
        IToDoListService toDoListService) //
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _toDoService = toDoService ?? throw new ArgumentNullException(nameof(toDoService));

        _toDoReportService = toDoReportService ?? throw new ArgumentNullException(nameof(toDoReportService));

        _toDoListService = toDoListService ?? throw new ArgumentNullException(nameof(toDoListService));

        _scenarios = scenarios;
        _scenarioContextRepository = contextRepository;
        
        //подписываюсь на события
        OnHandleUpdateStarted += HandleStart;
        OnHandleUpdateCompleted += HandleComplete;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (update.Message != null)
        {
            await OnMessage(botClient, update, update.Message, ct);
        }
        else if (update.CallbackQuery != null)
        {
            await OnCallbackQuery(botClient, update, update.CallbackQuery, ct);
        }
        else
        {
            await OnUnknown();
        }
    }

    private async Task OnMessage(ITelegramBotClient botClient, Update update, Message message, CancellationToken ct)
    {
        var messageData = Helpers.Extensions.MessageGetData(update, ct);
        
        try
        {
            var currentUser = await _userService.GetUser(messageData.TelegramUserId, ct);

            //var currentUserTaskList = currentUser != null
            //    ? await _toDoService.GetAllByUserId(currentUser.UserId, ct)
            //    : null;

            if (update.Message.Id == 1)
            {
                await botClient.SendMessage(messageData.Chat, $"Привет! Это Todo List Bot! \n", cancellationToken: ct);
                return;
            }

            if (currentUser == null)
            {
                if (messageData.UserInput != "/start")
                {
                    await botClient.SendMessage(messageData.Chat, "До регистрации доступна только команда /start. Нажмите на кнопку ниже или введите /start", replyMarkup: Helper.keyboardStart, cancellationToken: ct);
                    return;
                }
            }

            //НАЧАЛО ОБРАБОТКИ СООБЩЕНИЯ
            OnHandleUpdateStarted?.Invoke(message.Text);

            (string inputCommand, string inputText, Guid taskGuid) = Helper.InputCheck(messageData.UserInput);

            messageData.UserInput = inputCommand.Replace("/", string.Empty);

            if (currentUser == null && messageData.UserInput != "start")
                messageData.UserInput = "unregistered user command";

            //получение значений команд типа Enum
            Commands command = Helper.GetEnumValue<Commands>(messageData.UserInput);

            //КОНЕЦ ОБРАБОТКИ СООБЩЕНИЯ
            OnHandleUpdateCompleted?.Invoke(message.Text);

            //Работа с командой cancel и сценариями
            if (command == Commands.Cancel)
                await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);

            var scenarioContext = await _scenarioContextRepository.GetContext(messageData.TelegramUserId, ct);

            if (scenarioContext != null)
            {
                await ProcessScenario(scenarioContext, update, ct);
                return;
            }

            switch (command)
            {
                case Commands.Start:
                    if (currentUser == null)
                        currentUser = await _userService.RegisterUser(messageData.TelegramUserId, update.Message.From.Username, ct);

                    // await botClient.SendMessage(messageData.Chat, "Спасибо за регистрацию", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    await Commands.Start.CommandsRender(currentUser, messageData.Chat, botClient, ct);
                    break;

                case Commands.Help:
                    await HandleShowHelpCommand(currentUser);
                    break;

                case Commands.Info:
                    await HandleShowInfoCommand();
                    break;

                case Commands.Addtask:
                    await ProcessScenario(
                        Helper.CreateScenarioContext(ScenarioType.AddTask, currentUser.UserId),
                        update,
                        ct);
                    break;

                case Commands.Cancel:
                    await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
                    await botClient.SendMessage(messageData.Chat, "Сценарий отменён. Выбирай что хочешь сделать?", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;

                case Commands.Show:
                    var lists = await _toDoListService.GetUserLists(currentUser.UserId, ct);
                    await botClient.SendMessage(messageData.Chat, "Выберите список", replyMarkup: Helper.GetSelectListKeyboardForShow(lists), cancellationToken: ct);
                    break;

                case Commands.Find:
                    var findedTasks = await _toDoService.Find(currentUser, inputText, ct);
                    await ShowTasks(currentUser.UserId, true, findedTasks);
                    break;

                case Commands.Report:
                    var (total, completed, active, generatedAt) = await _toDoReportService.GetUserStats(currentUser.UserId, ct);
                    await botClient.SendMessage(messageData.Chat, $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;

                case Commands.Exit:
                    await botClient.SendMessage(messageData.Chat, "Нажмите CTRL+C (Ввод) для остановки бота", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
                default:
                    await botClient.SendMessage(messageData.Chat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
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
            await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
            await botClient.SendMessage(messageData.Chat, "Текущий сценарий завершен. Нужно почистить список задач.", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        }
        //catch (TaskLengthLimitException ex)
        //{
        //    await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
        //    await botClient.SendMessage(currentChat, "Нужно почистить список задач.", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        //}
        catch (DuplicateTaskException ex)
        {
            await botClient.SendMessage(messageData.Chat, ex.Message, cancellationToken: ct);
            await botClient.SendMessage(messageData.Chat, "Введите название задачи заново или нажмите кнопку отмены:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
            return;
        }
        catch (EmptyTaskListException ex)
        {
            await botClient.SendMessage(messageData.Chat, ex.Message, cancellationToken: ct);
            await botClient.SendMessage(messageData.Chat, "Введите название задачи заново или нажмите кнопку отмены:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
        }
        #endregion

        catch (Exception)
        {
            await botClient.SendMessage(messageData.Chat, $"Произошла непредвиденная ошибка", cancellationToken: ct);
            throw;
        }
        #region МЕТОДЫ КОМАНД
        async Task ShowTasks(Guid userId, bool isActive = false, IReadOnlyList<ToDoItem>? tasks = null)
        {
            var tasksList = tasks ?? (isActive
                ? await _toDoService.GetAllByUserId(userId, ct)
                : await _toDoService.GetActiveByUserId(userId, ct));

            if (tasksList.Count == 0)
            {
                string emptyMessage = isActive ? "Список задач пуст\n" : "Aктивных задач нет";
                await botClient.SendMessage(messageData.Chat, emptyMessage, replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                return;
            }

            string message = tasks != null ? "Список найденных задач:"
                : (isActive ? "Список всех задач:" : "Список активных задач:");

            await botClient.SendMessage(messageData.Chat, message, replyMarkup: Helper.keyboardReg, cancellationToken: ct);

            if (isActive)
                await Helper.TasksListRender(tasksList, botClient, messageData.Chat, messageData.MessageId, isActive, ct);
            else
                await Helper.TasksListRender(tasksList, botClient, messageData.Chat, messageData.MessageId, ct);            
        }

        async Task HandleShowHelpCommand(ToDoUser user)
        {
            if (user == null)
            {
                await botClient.SendMessage(messageData.Chat, $"Незнакомец, это Todo List Bot - телеграм бот записи дел.\n" +
                                                              $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                                                              $"Введя команду \"/help\" ты получишь справку о командах\n" +
                                                              $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                                                              $"Введя команду \"/exit\" бот попрощается и завершит работу\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
            }
            else
            {
                await botClient.SendMessage(messageData.Chat, $"{user.TelegramUserName}, это Todo List Bot - телеграм бот записи дел.\n" +
                                                              $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                                                              $"Введя команду \"/help\" ты получишь справку о командах\n" +
                                                              $"Введя команду \"/addtask\" будет предложено ввести название задачи и при успешном вводе, задача будет добавлена\n" +
                                                              $"Введя команду \"/cancel\" ты сможешь отменить отменить добавление новой задачи \n" +
                                                              $"Введя команду \"/show\" ты сможешь увидеть список активных задач в списках\n" +
                                                              $"Введя команду \"/find\" *название задачи*\" ты сможешь увидеть список всех задач начинающихся с названия задачи\n" +
                                                              $"Введя команду \"/report\" ты получишь отчёт по задачам\n" +
                                                              $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                                                              $"Введя команду \"/exit\" бот попрощается и завершит работу\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
            }
        }

        async Task HandleShowInfoCommand()
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            await botClient.SendMessage(messageData.Chat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
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
                await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
            else
                await _scenarioContextRepository.SetContext(messageData.TelegramUserId, context, ct);
        }
        #endregion
        
    }
    private async Task OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery, CancellationToken ct)
    {
        var messageData = Helpers.Extensions.MessageGetData(update, ct);

        ToDoItem currentTask = null;

        try
        {
            var currentUser = await _userService.GetUser(messageData.TelegramUserId, ct);

            if (currentUser == null && messageData.UserInput != "/start")
            {
                await botClient.SendMessage(
                    messageData.Chat, 
                    "До регистрации доступна только команда /start. Нажмите на кнопку ниже или введите /start", 
                    replyMarkup: Helper.keyboardStart, 
                    cancellationToken: ct);
                return;
            }

            var callbackDto = CallbackDto.FromString(messageData.UserInput);

            var callbackPagedListDto = PagedListCallbackDto.FromString(messageData.UserInput);

            //НАЧАЛО ОБРАБОТКИ СООБЩЕНИЯ
            OnHandleUpdateStarted?.Invoke(callbackQuery.Message.Text);

            //получение значений команд типа Enum
            Commands command = Helper.GetEnumValue<Commands>(messageData.UserInput);
            //ScenarioType scenarioType = Helper.GetEnumValue<ScenarioType>(input);

            //КОНЕЦ ОБРАБОТКИ СООБЩЕНИЯ
            OnHandleUpdateCompleted?.Invoke(callbackQuery.Message.Text);

            //Работа с командой cancel и сценариями
            if (command == Commands.Cancel)
                await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);

            var scenarioContext = await _scenarioContextRepository.GetContext(messageData.TelegramUserId, ct);

            if (scenarioContext != null)
            {
                if (callbackPagedListDto.ToDoListId.HasValue)
                    scenarioContext.Data["List"] = await _toDoListService.Get(callbackPagedListDto.ToDoListId.Value, ct);

                await ProcessScenario(scenarioContext, update, ct);
                return;
            }         

            switch (callbackDto.Action)
            {
                case "show":
                    var activetoDoItems = await GetKeyValuePairTasksCollection(
                        currentUser.UserId,
                        callbackPagedListDto.ToDoListId,
                        ToDoItemState.Active,
                        ct);
                    
                        await botClient.EditMessageText(
                        messageData.Chat,
                        messageData.MessageId,
                        "Список задач",
                        replyMarkup: await BuildPagedButtons(activetoDoItems, callbackPagedListDto),
                        cancellationToken: ct);
                    break;

                case "addlist":
                    await ProcessScenario(
                        Helper.CreateScenarioContext(ScenarioType.AddList, currentUser.UserId), 
                        update,
                        ct);
                    break;

                case "showtask":
                    currentTask = await GetToDoItemFromCallbackDto(messageData.UserInput, ct);

                    await botClient.SendMessage(
                        messageData.Chat,
                        $"{currentTask.Name}: \n\nСрок выполнения: {currentTask.Deadline}\nВремя создания: {currentTask.CreatedAt}",
                        replyMarkup: Helper.GetToDoItemKeyboard(currentTask), 
                        cancellationToken: ct);
                    break;

                case "show_completed":
                    var completedtoDoItems = await GetKeyValuePairTasksCollection(
                        currentUser.UserId,
                        callbackPagedListDto.ToDoListId,
                        ToDoItemState.Completed,
                        ct);

                    await botClient.EditMessageText(
                        messageData.Chat,
                        messageData.MessageId,
                        "Список выполненных задач",
                        replyMarkup: await BuildPagedButtons(completedtoDoItems, callbackPagedListDto),
                        cancellationToken: ct);
                    break;

                case "completetask":
                    currentTask = await GetToDoItemFromCallbackDto(messageData.UserInput, ct);

                    await _toDoService.MarkCompleted(currentTask.Id, ct);

                    await botClient.EditMessageText(
                        messageData.Chat,
                        messageData.MessageId,
                        $"{currentTask.Name}: \n\nСрок выполнения: {currentTask.Deadline}\nВремя создания: {currentTask.CreatedAt}",
                        replyMarkup: default,
                        cancellationToken: ct);

                    await botClient.SendMessage(messageData.Chat, "Задача выполнена", cancellationToken: ct);
                    break;

                case "deletetask":
                    await ProcessScenario(
                        Helper.CreateScenarioContext(ScenarioType.DeleteTask, currentUser.UserId),
                        update,
                        ct);
                    break;

                case "deletelist":
                    await ProcessScenario(
                        Helper.CreateScenarioContext(ScenarioType.DeleteList, currentUser.UserId),
                        update,
                        ct);
                    break;

                default:
                    await botClient.SendMessage(
                        messageData.Chat, 
                        "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n", 
                        replyMarkup: Helper.keyboardReg, 
                        cancellationToken: ct);

                    await Commands.Start.CommandsRender(currentUser, messageData.Chat, botClient, ct);
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
            await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
            await botClient.SendMessage(messageData.Chat, "Текущий сценарий завершен. Нужно почистить список задач.", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        }
        //catch (TaskLengthLimitException ex)
        //{
        //    await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
        //    await botClient.SendMessage(currentChat, "Нужно почистить список задач.", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
        //}
        catch (DuplicateTaskException ex)
        {
            await botClient.SendMessage(messageData.Chat, ex.Message, cancellationToken: ct);
            await botClient.SendMessage(messageData.Chat, "Введите название задачи заново или нажмите кнопку отмены:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
            return;
        }
        catch (EmptyTaskListException ex)
        {
            await botClient.SendMessage(messageData.Chat, ex.Message, cancellationToken: ct);
            await botClient.SendMessage(messageData.Chat, "Введите название задачи заново или нажмите кнопку отмены:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
        }
        #endregion

        catch (Exception)
        {
            //await botClient.SendMessage(currentChat, $"Произошла непредвиденная ошибка", cancellationToken: ct);
            throw;
        }

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

        /// <summary>
        /// Работает со сценарием, устанавливает или сбрасывает контекст. Получает сценарий, получает результат обработки сценария в зависимости от ввода пользователя
        /// </summary>
        /// <param name="context">Контекст в зависимотси от типа сценария</param>
        async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenario = GetScenario(context.CurrentScenario);

            var scenarioResult = await scenario.HandleMessageAsync(botClient, context, update, ct);

            if (scenarioResult == ScenarioResult.Completed)
                await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
            else
                await _scenarioContextRepository.SetContext(messageData.TelegramUserId, context, ct);
        }
        #endregion
    }

    private async Task OnUnknown()
    {
        throw new ArgumentException("Получен неизветсный тип сообщения");
    }
    
    // private async Task HandleStartCommand(ToDoUser user)
    // {
    //     if (user == null)
    //         user = await _userService.RegisterUser(telegramUserId, update.Message.From.Username, ct);
    //
    //     await botClient.SendMessage(currentChat, "Спасибо за регистрацию", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
    //     await Commands.Start.CommandsRender(user, currentChat, botClient, ct);
    // }

    private async Task<IReadOnlyList<KeyValuePair<string, string>>> GetKeyValuePairTasksCollection(
        Guid userId, 
        Guid? toDoListId, 
        ToDoItemState state,
        CancellationToken ct)
    {
        var toDoItems = await _toDoService.GetByUserIdAndList(userId, toDoListId, ct);

        return toDoItems
            .Where(item => item.State == state)
            .ToReadOnlyKeyValueList(
                item => item.Id.ToString(),
                item => item.Name);
    }

    private async Task<InlineKeyboardMarkup> BuildPagedButtons(
        IReadOnlyList<KeyValuePair<string, string>> callbackData,
        PagedListCallbackDto listDto)
    {
        var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();

        var totalPages = (int)Math.Ceiling(((double)callbackData.Count / _pageSize));

        var currentPageTasks = callbackData.GetBatchByNumber(_pageSize, listDto.Page);

        Helper.GetToDoItemListKeyboardWithPagination(currentPageTasks, keyboardRows, listDto, totalPages, true);

        return new InlineKeyboardMarkup(keyboardRows);
    }
    
    private async Task<ToDoItem?> GetToDoItemFromCallbackDto(string input, CancellationToken ct)
    {
        var callbackItemDto = ToDoItemCallbackDto.FromString(input);

        if (callbackItemDto.ToDoItemId != null)
            return await _toDoService.Get(callbackItemDto.ToDoItemId.Value, ct);

        return null;
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