using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Exceptions;
using NailBot.Core.Services;
using NailBot.Helpers;
using NailBot.TelegramBot.Scenarios;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailBot.TelegramBot.MessageHandlers;

public class MessageHandler : IMessageHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoListService _toDoListService;
    private readonly IToDoReportService _toDoReportService;
    private readonly IScenarioContextRepository _scenarioContextRepository;
    private readonly IEnumerable<IScenario> _scenarios;
    
    public MessageHandler(
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
    
    
    
    public async Task HandleAsync(ITelegramBotClient botClient, Update update, Message message, CancellationToken ct)
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
            // OnHandleUpdateStarted?.Invoke(message.Text);

            (string inputCommand, string inputText, Guid taskGuid) = Helper.InputCheck(messageData.UserInput);

            messageData.UserInput = inputCommand.Replace("/", string.Empty);

            if (currentUser == null && messageData.UserInput != "start")
                messageData.UserInput = "unregistered user command";

            //получение значений команд типа Enum
            Commands command = Helper.GetEnumValue<Commands>(messageData.UserInput);

            //КОНЕЦ ОБРАБОТКИ СООБЩЕНИЯ
            // OnHandleUpdateCompleted?.Invoke(message.Text);

            //Работа с командой cancel и сценариями
            // if (command == Commands.Cancel)
            //     await _scenarioContextRepository.ResetContext(messageData.TelegramUserId, ct);

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
}