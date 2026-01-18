using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Exceptions;
using NailBot.Core.Services;
using NailBot.Helpers;
using NailBot.TelegramBot.Dto;
using NailBot.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NailBot.TelegramBot.MessageHandlers;

public class CallbackQueryHandler : ICallbackQueryHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoListService _toDoListService;
    private readonly IToDoReportService _toDoReportService;
    private readonly IScenarioContextRepository _scenarioContextRepository;
    private readonly IEnumerable<IScenario> _scenarios;
    
    //количество кнопок задач на 1 странице
    int _pageSize = 5;
    
    public CallbackQueryHandler(
        ITelegramBotClient botClient,
        IUserService userService,
        IToDoService toDoService,
        IToDoListService toDoListService,
        IToDoReportService toDoReportService,
        IScenarioContextRepository scenarioContextRepository,
        IEnumerable<IScenario> scenarios)
    {
        _botClient = botClient;
        _userService = userService;
        _toDoService = toDoService;
        _toDoListService = toDoListService;
        _toDoReportService = toDoReportService;
        _scenarioContextRepository = scenarioContextRepository;
        _scenarios = scenarios;
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
            // OnHandleUpdateStarted?.Invoke(callbackQuery.Message.Text);

            //получение значений команд типа Enum
            Commands command = Helper.GetEnumValue<Commands>(messageData.UserInput);
            //ScenarioType scenarioType = Helper.GetEnumValue<ScenarioType>(input);

            //КОНЕЦ ОБРАБОТКИ СООБЩЕНИЯ
            // OnHandleUpdateCompleted?.Invoke(callbackQuery.Message.Text);

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


}