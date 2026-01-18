using Microsoft.Extensions.Options;
using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using NailBot.TelegramBot.Scenarios;
using NailBot.Core.Exceptions;
using NailBot.Domain;
using NailBot.Options;
using NailBot.TelegramBot.Dto;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace NailBot.TelegramBot;

internal delegate void MessageEventHandler(string message);

internal class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoReportService _toDoReportService;
    private readonly IToDoListService _toDoListService;
    private readonly IMessageService _messageService;
    
    private readonly IEnumerable<IScenario> _scenarios;
    private readonly IScenarioContextRepository _scenarioContextRepository;
    
    private readonly int _pageSize;
    
    public event MessageEventHandler? OnHandleUpdateStarted;
    public event MessageEventHandler? OnHandleUpdateCompleted;
  
    public UpdateHandler(
        IUserService userService, 
        IToDoService toDoService, 
        IToDoReportService toDoReportService,
        IEnumerable<IScenario> scenarios, 
        IScenarioContextRepository contextRepository,
        IToDoListService toDoListService, 
        IMessageService messageService,
        IOptions<PaginationOptions> options)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _toDoService = toDoService ?? throw new ArgumentNullException(nameof(toDoService));
        _toDoReportService = toDoReportService ?? throw new ArgumentNullException(nameof(toDoReportService));
        _toDoListService = toDoListService ?? throw new ArgumentNullException(nameof(toDoListService));
        _messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
        
        _scenarios = scenarios;
        _scenarioContextRepository = contextRepository;
        
        _pageSize = options.Value.PageSize;
        
        OnHandleUpdateStarted += HandleStart;
        OnHandleUpdateCompleted += HandleComplete;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        if (update.Message != null)
        {
            await OnMessage(botClient, update, ct);
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

    private async Task OnMessage(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        var messageData = Helpers.Extensions.MessageGetData(update, ct);
        
        try
        {
            OnHandleUpdateStarted?.Invoke(messageData.UserInput);
            
            var context = await _scenarioContextRepository.GetContext(messageData.TelegramUserId, ct);

            await TryHandleOnMessageCommandAsync(messageData, context, update, ct);

            OnHandleUpdateCompleted?.Invoke(messageData.UserInput);
        }
        catch (Exception ex)
        {
            await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
            await HandleErrorAsync(botClient, ex, HandleErrorSource.HandleUpdateError, ct);
        }
    }

    private async Task OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery, CancellationToken ct)
    {
        var messageData = Helpers.Extensions.MessageGetData(update, ct);

        try
        {
            OnHandleUpdateStarted?.Invoke(callbackQuery.Message.Text);

            var context = await _scenarioContextRepository.GetContext(messageData.TelegramUserId, ct);
            
            await TryHandleOnCallbackQueryAsync(messageData, context, update, ct);
            
            OnHandleUpdateCompleted?.Invoke(callbackQuery.Message.Text);
        }
        catch (Exception ex)
        {
            await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
            await HandleErrorAsync(botClient, ex, HandleErrorSource.HandleUpdateError, ct);
        }
    }

    
    private async Task TryHandleOnMessageCommandAsync(MessageData messageData, ScenarioContext? context, Update update, CancellationToken ct)
    {
        var currentUser = await _userService.GetUser(messageData.TelegramUserId, ct);
            
        if (currentUser == null && (messageData.UserInput != "/start" && messageData.UserInput != "Старт"))
        {
            await _messageService.SendMessage(messageData.Chat, Constants.StartBotMessage, replyMarkup: Helper.keyboardStart, ct);
            return;
        }
        
        (var inputCommand, var inputText) = Helper.InputCheck(messageData.UserInput);

        messageData.UserInput = inputCommand.Replace("/", string.Empty);
        
        var command = Helper.GetEnumValue<Commands>(messageData.UserInput);
        
        if (command == Commands.Cancel)
            await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
        
        if (context != null)
        {
            await ProcessScenario(context, messageData, update, ct);
            return;
        }
        
        switch (command)
        {
            case Commands.Start:
                await HandleStartCommand(update.Message, currentUser, ct);  
                break;
            case Commands.Help:
                await HandleShowHelpCommand(messageData.Chat, ct);
                break;
            case Commands.Info:
                await HandleShowInfoCommand(messageData.Chat, ct);
                break;
            case Commands.AddTask:
                await HandleAddTaskCommand(messageData, update, currentUser.UserId, ct);
                break;
            case Commands.Cancel:
                await HandleCancelCommand(messageData, ct);
                break;
            case Commands.Show:
                await HandleShowCommand(messageData.Chat, currentUser.UserId, ct);
                break;
            case Commands.Find:
                await HandleFindCommand(messageData.Chat, currentUser, inputText, ct); 
                break;
            case Commands.Report:
                await HandleReportCommand(messageData.Chat, currentUser.UserId, ct); 
                break;
            case Commands.Exit:
                await _messageService.SendMessage(messageData.Chat, Constants.ExitBotMessage, replyMarkup: Helper.keyboardReg, ct);
                break;
            default:
                await _messageService.SendMessage(messageData.Chat, Constants.ErrorMessage, replyMarkup: Helper.keyboardReg, ct);
                break;
        }
    }
    
    private async Task TryHandleOnCallbackQueryAsync(MessageData messageData, ScenarioContext? context, Update update,
        CancellationToken ct)
    {
        var currentUser = await _userService.GetUser(messageData.TelegramUserId, ct);

        if (currentUser == null && (messageData.UserInput != "/start" && messageData.UserInput != "Старт"))
        {
            await _messageService.SendMessage(messageData.Chat, "Для запуска бота необходимо нажать на кнопку ниже или ввести /start", replyMarkup: Helper.keyboardStart,  ct);
            return;
        }

        var callbackDto = CallbackDto.FromString(messageData.UserInput);

        var callbackPagedListDto = PagedListCallbackDto.FromString(messageData.UserInput);
            
        var command = Helper.GetEnumValue<Commands>(messageData.UserInput);
            
        if (command == Commands.Cancel)
            await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
        
        if (context != null)
        {
            if (callbackPagedListDto.ToDoListId.HasValue)
                context.Data["List"] = await _toDoListService.Get(callbackPagedListDto.ToDoListId.Value, ct);

            await ProcessScenario(context, messageData, update, ct);
            return;
        }         

        switch (callbackDto.Action)
        {
            case "show":
                await HandleShowAction(messageData, currentUser.UserId, ct);
                break;
            case "addlist":
                await HandleAddListAction(messageData, update, currentUser.UserId, ct);
                break;
            case "showtask":
                await HandleShowTaskAction(messageData, ct);
                break;
            case "show_completed":
                await HandleShowCompletedAction(messageData, currentUser.UserId, ct);
                break;
            case "completetask":
                await HandleCompleteTaskAction(messageData, ct);
                break;
            case "deletetask":
                await HandleDeleteTaskAction(messageData, update, currentUser.UserId, ct);
                break;
            case "deletelist":
                await HandleDeleteListAction(messageData, update, currentUser.UserId, ct);
                break;
            default:
                await _messageService.SendMessage(messageData.Chat, Constants.ErrorMessage, replyMarkup: Helper.keyboardReg, ct);
                break;
        }
    }
    
    #region МЕТОДЫ ON MESSAGE КОМАНД
    private async Task HandleStartCommand(Message updateMessage, ToDoUser? user, CancellationToken ct)
    {
        if (user == null)
        {
            await _userService.RegisterUser(updateMessage.From.Id, updateMessage.From.Username, ct);
            await _messageService.SendMessage(updateMessage.Chat, Constants.RegisterMessage, replyMarkup: Helper.keyboardReg, ct);
        }
        var commandsList = Helpers.Extensions.CommandsRender();
        await _messageService.SendMessage(updateMessage.Chat, commandsList, replyMarkup: Helper.keyboardReg, ct);
    }
    private async Task HandleAddTaskCommand(MessageData messageData, Update update, Guid userId, CancellationToken ct)
    {                    
        await ProcessScenario(
            Helper.CreateScenarioContext(ScenarioType.AddTask, userId), 
            messageData, 
            update, 
            ct);
    }
    private async Task HandleCancelCommand(MessageData messageData, CancellationToken ct)
    {                    
        await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
        await _messageService.SendMessage(messageData.Chat, Constants.CancelScenarioMessage, replyMarkup: Helper.keyboardReg, ct);
    }
    private async Task HandleShowCommand(Chat chat, Guid userId, CancellationToken ct)
    {
        var lists = await _toDoListService.GetUserLists(userId, ct);
        await _messageService.SendMessage(chat, "Выберите список", replyMarkup: Helper.GetSelectListKeyboardForShow(lists), ct);
    }
    private async Task HandleShowTasksCommand(Chat chat, Guid userId, CancellationToken ct, bool isActive = false,  IReadOnlyList<ToDoItem>? tasks = null)
    {
        var tasksList = tasks ?? (isActive
            ? await _toDoService.GetActiveByUserId(userId, ct)
            : await _toDoService.GetAllByUserId(userId, ct));
    
        if (tasksList.Count == 0)
        {
            string emptyMessage = isActive ? "Список задач пуст\n" : "Aктивных задач нет";
            await _messageService.SendMessage(chat, emptyMessage, replyMarkup: Helper.keyboardReg, ct);
            return;
        }
    
        string message = tasks != null ? "Список найденных задач:"
            : (isActive ? "Список всех задач:" : "Список активных задач:");
        
        await _messageService.SendMessage(chat, message, replyMarkup: Helper.keyboardReg, ct);
        
        int taskCounter = 0;
        
        foreach (var task in tasksList)
        {
            taskCounter++;
            await _messageService.SendMessage(chat, $"{taskCounter}) ({task.State}) {task.Name} - {task.CreatedAt}", ct);
            await _messageService.SendMessage(chat, $"```Id\n{task.Id}```", parseMode: ParseMode.MarkdownV2, ct);
        }
    }
    private async Task HandleFindCommand(Chat chat, ToDoUser user, string inputText, CancellationToken ct)
    {
        var findedTasks = await _toDoService.Find(user, inputText, ct);

        if (findedTasks.Count != 0)
        {
            await HandleShowTasksCommand(chat, user.UserId, ct, true, findedTasks);
            return;
        }
        await _messageService.SendMessage(chat, Constants.TaskNotFoundMessage, replyMarkup: Helper.keyboardReg, ct);
    }
    private async Task HandleShowHelpCommand(Chat chat, CancellationToken ct)
    {
        await _messageService.SendMessage(chat, Constants.CommandDescriptionMessage, replyMarkup: Helper.keyboardReg, ct);
    }
    private async Task HandleShowInfoCommand(Chat chat, CancellationToken ct)
    {
        await _messageService.SendMessage(chat, Constants.InfoMessage, replyMarkup: Helper.keyboardReg, ct);
    }
    private async Task HandleReportCommand(Chat chat, Guid userId, CancellationToken ct)
    {
        var (total, completed, active, generatedAt) = await _toDoReportService.GetUserStats(userId, ct);
        var message = $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};";
        await _messageService.SendMessage(chat, message, replyMarkup: Helper.keyboardReg, ct);
    }
    #endregion
    
    #region МЕТОДЫ ON CALLBACKQUERY КОМАНД
    private async Task HandleShowAction(MessageData messageData, Guid userId, CancellationToken ct)
    {
        var callbackPagedListDto = PagedListCallbackDto.FromString(messageData.UserInput);
        
        var activetoDoItems = await GetKeyValuePairTasksCollection(
            userId,
            callbackPagedListDto.ToDoListId,
            ToDoItemState.Active,
            ct);
                    
        await _messageService.EditMessageText(
            messageData.Chat,
            messageData.MessageId,
            "Список задач",
            await BuildPagedButtons(activetoDoItems, callbackPagedListDto),
            ct);
    }
    private async Task HandleAddListAction(MessageData messageData, Update update, Guid userId, CancellationToken ct)
    {
        await ProcessScenario(
            Helper.CreateScenarioContext(ScenarioType.AddList, userId), 
            messageData,
            update,
            ct);
    }
    private async Task HandleShowCompletedAction(MessageData messageData, Guid userId, CancellationToken ct)
    {
        var callbackPagedListDto = PagedListCallbackDto.FromString(messageData.UserInput);
        
        var completedtoDoItems = await GetKeyValuePairTasksCollection(
            userId,
            callbackPagedListDto.ToDoListId,
            ToDoItemState.Completed,
            ct);

        await _messageService.EditMessageText(
            messageData.Chat,
            messageData.MessageId,
            "Список выполненных задач",
            await BuildPagedButtons(completedtoDoItems, callbackPagedListDto),
            ct);
    }
    private async Task HandleCompleteTaskAction(MessageData messageData, CancellationToken ct)
    {
        var currentTask = await GetToDoItemFromCallbackDto(messageData.UserInput, ct);

        await _toDoService.MarkCompleted(currentTask.Id, ct);

        await _messageService.EditMessageText(
            messageData.Chat,
            messageData.MessageId,
            $"{currentTask.Name}: \n\nСрок выполнения: {currentTask.Deadline}\nВремя создания: {currentTask.CreatedAt}",
            default,
            ct);

        await _messageService.SendMessage(messageData.Chat, "Задача выполнена", ct);
    }
    private async Task HandleDeleteTaskAction(MessageData messageData, Update update, Guid userId, CancellationToken ct)
    {
        await ProcessScenario(
            Helper.CreateScenarioContext(ScenarioType.DeleteTask, userId),
            messageData,
            update,
            ct);
    }
    private async Task HandleDeleteListAction(MessageData messageData, Update update, Guid userId, CancellationToken ct)
    {
        await ProcessScenario(
            Helper.CreateScenarioContext(ScenarioType.DeleteList, userId), 
            messageData,
            update,
            ct);
    }
    private async Task HandleShowTaskAction(MessageData messageData, CancellationToken ct)
    {
        var currentTask = await GetToDoItemFromCallbackDto(messageData.UserInput, ct);

        await _messageService.SendMessage(
            messageData.Chat,
            $"{currentTask.Name}: \n\nСрок выполнения: {currentTask.Deadline}\nВремя создания: {currentTask.CreatedAt}",
            replyMarkup: Helper.GetToDoItemKeyboard(currentTask), 
            ct);
    }
    #endregion
    
    #region МЕТОДЫ СЦЕНАРИЯ
    // /// <summary>
    // /// Возвращает экземпляр сценария по указанному типу.
    // /// </summary>
    // /// <param name="scenario">Тип сценария из перечисления ScenarioType</param>
    // /// <returns>Реализация интерфейса IScenario для запрошенного сценария</returns>
    // /// <exception cref="NotSupportedException">Выбрасывается при передаче неподдерживаемого значения ScenarioType</exception>
    IScenario GetScenario(ScenarioType scenario)
    {
        var currentScenario = _scenarios.FirstOrDefault(s => s.CanHandle(scenario));
        return currentScenario ?? throw new NotSupportedException($"Сценарий {scenario} не поддерживается");
    }

    // /// <summary>
    // /// Работает со сценарием, устанавливает или сбрасывает контекст. Получает сценарий, получает результат обработки сценария в зависимости от ввода пользователя
    // /// </summary>
    // /// <param name="context">Контекст в зависимотси от типа сценария</param>
    async Task ProcessScenario(ScenarioContext context, MessageData messageData, Update update, CancellationToken ct)
    {
        var scenario = GetScenario(context.CurrentScenario);

        var scenarioResponse = await scenario.HandleMessageAsync(context, messageData.Chat, update, ct);
        
        if (scenarioResponse.IsEdit)
        {
            if (scenarioResponse.EditMessages.Count > 1)
            {
                if (!scenarioResponse.HasText)
                    await _messageService.EditMultiMessage(messageData.Chat, scenarioResponse.EditMessages, ct);
                else
                    await _messageService.EditMultiMessageWithText(messageData.Chat, scenarioResponse.Message, scenarioResponse.EditMessages, ct);
            }
            else
            { 
                await _messageService.EditMessage(messageData.Chat, update.Message.Id, (InlineKeyboardMarkup)scenarioResponse.Keyboard, ct);  
            }
        }
        else
        {
            if (scenarioResponse.Messages != null && scenarioResponse.Messages.Count > 1)
                await _messageService.SendMultiMessage(scenarioResponse.Chat, scenarioResponse.Messages, ct);
            else
                await _messageService.SendMessage(scenarioResponse.Chat, scenarioResponse.Message, scenarioResponse.Keyboard, ct);
        }

        if (scenarioResponse.Result == ScenarioResult.Completed)
            await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);
        else
            await _scenarioContextRepository.SetContext(messageData.TelegramUserId, context, ct);
    }
    #endregion
    
    private async Task OnUnknown()
    {
        throw new ArgumentException("Получен неизветсный тип сообщения");
    }

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