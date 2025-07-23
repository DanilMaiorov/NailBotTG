using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailBot.TelegramBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        private readonly IToDoListService _toDoListService;

        public AddTaskScenario(IUserService userService, IToDoService toDoService, IToDoListService toDoListService)
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            //верну выполненный сценарий если придёт какая-то левая инфа
            if (update.Message == null && update.CallbackQuery == null)
                return ScenarioResult.Completed;

            (Chat? currentChat, string? currentUserInput, ToDoUser? currentUser) = await Helper.HandleMessageAsyncGetData(update, context, _userService, ct);

            switch (context.CurrentStep)
            {
                case null:
                    return await HandleInitialStep(botClient, context, currentUser, currentChat, ct);

                case "Name":
                    return await HandleNameStep(botClient, context, currentUser, currentChat, currentUserInput, ct);

                case "Deadline":
                    return await HandleDeadlineStep(botClient, context, currentUser, currentChat, currentUserInput, ct);

                case "List":
                    return await HandleChooseListStep(botClient, context, currentUser, currentChat, ct);

                default:
                    await botClient.SendMessage(currentChat, "Неизвестный шаг сценария", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
             }
            return ScenarioResult.Completed;
        }

        private async Task<ScenarioResult> HandleInitialStep(ITelegramBotClient botClient, ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            context.Data["User"] = user;

            await botClient.SendMessage(chat, "Введите название задачи:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);

            context.CurrentStep = "Name";

            return ScenarioResult.Transition;
        }
        private async Task<ScenarioResult> HandleNameStep(ITelegramBotClient botClient, ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            context.Data["Name"] = await _toDoService.ThrowIfHasDuplicatesOrWhiteSpace(userInput, user.UserId, ct);

            await botClient.SendMessage(chat, "Введите дедлайн задачи в формате dd.MM.yyyy:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);

            context.CurrentStep = "Deadline";

            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> HandleDeadlineStep(ITelegramBotClient botClient, ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            if (!Helper.TryParseUserDate(userInput, out DateTime deadline))
            {
                await botClient.SendMessage(chat, "Неверный формат даты. Попробуйте ещё раз в формате dd.MM.yyyy:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
                return ScenarioResult.Transition;
            }

            context.Data["Deadline"] = deadline;

            context.CurrentStep = "List";

            var lists = await _toDoListService.GetUserLists(user.UserId, ct);

            await botClient.SendMessage(chat, "Выберите список", replyMarkup: Helper.GetSelectListKeyboardForAdd(lists), cancellationToken: ct);

            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> HandleChooseListStep(ITelegramBotClient botClient, ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            if (!context.Data.TryGetValue("List", out var toDoListObj))
                throw new InvalidOperationException("Список не найден в контексте");

            var toDoList = (ToDoList)toDoListObj;

            var toDoItem = await _toDoService.Add(
                (ToDoUser)context.Data["User"],
                (string)context.Data["Name"],
                (DateTime)context.Data["Deadline"],
                toDoList,
                ct);

            if (toDoList != null)
                await botClient.SendMessage(chat, $"Задача \"{toDoList.Name}\" добавлена в список \"{toDoList.Name}\".\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
            else
                await botClient.SendMessage(chat, $"Задача \"{toDoList.Name}\" добавлена в общий список.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);

            return ScenarioResult.Completed;
        }
    }
}