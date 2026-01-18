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

        public async Task<ScenarioResponse> HandleMessageAsync(ScenarioContext context, Chat chat, Update update, CancellationToken ct)
        {
            //верну выполненный сценарий если придёт какая-то левая инфа
            if (update.Message == null && update.CallbackQuery == null)
                return new ScenarioResponse(ScenarioResult.Completed, chat)
                {
                    Message = "Это ToDoList Bot", //тут докрутить
                    Keyboard = Helper.keyboardReg
                };

            (Chat? currentChat, string? currentUserInput, int currentMessageId, ToDoUser? currentUser) = await Helper.HandleMessageAsyncGetData(update, context, ct, _userService);

            switch (context.CurrentStep)
            {
                case null:
                    return await HandleInitialStep(context, currentUser, currentChat, ct);

                case "Name":
                    return await HandleNameStep(context, currentUser, currentChat, currentUserInput, ct);

                case "Deadline":
                    return await HandleDeadlineStep(context, currentUser, currentChat, currentUserInput, ct);

                case "List":
                    return await HandleChooseListStep(context, currentUser, currentChat, ct);

                default:
                    return await HandleDefaultStep(currentChat, ct);
             }
        }

        private async Task<ScenarioResponse> HandleInitialStep(ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            await Task.Delay(1, ct);
            
            context.Data["User"] = user;

            context.CurrentStep = "Name";
            
            return new ScenarioResponse(ScenarioResult.Transition, chat)
            {
                Message = "Введите название задачи:",
                Keyboard = Helper.keyboardCancel
            };
        }
        private async Task<ScenarioResponse> HandleNameStep(ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            context.Data["Name"] = await _toDoService.ThrowIfHasDuplicatesOrWhiteSpace(userInput, user.UserId, ct);

            context.CurrentStep = "Deadline";

            return new ScenarioResponse(ScenarioResult.Transition, chat)
            {
                Message = "Введите дедлайн задачи в формате dd.MM.yyyy:",
                Keyboard = Helper.keyboardCancel
            };
        }

        private async Task<ScenarioResponse> HandleDeadlineStep(ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            if (!Helper.TryParseUserDate(userInput, out DateTime deadline))
            {
                return new ScenarioResponse(ScenarioResult.Transition, chat)
                {
                    Message = "Неверный формат даты. Попробуйте ещё раз в формате dd.MM.yyyy:",
                    Keyboard = Helper.keyboardCancel
                };
            }

            context.Data["Deadline"] = deadline;

            context.CurrentStep = "List";

            var lists = await _toDoListService.GetUserLists(user.UserId, ct);
            
            return new ScenarioResponse(ScenarioResult.Transition, chat)
            {
                Message = "Выберите список",
                Keyboard = Helper.GetSelectListKeyboardForAdd(lists)
            };
        }

        private async Task<ScenarioResponse> HandleChooseListStep(ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            var toDoList = context.Data.TryGetValue("List", out var toDoListObj) ? (ToDoList)toDoListObj : null;
            var toDoItemName = (string)context.Data["Name"];
            
            await _toDoService.Add(
                (ToDoUser)context.Data["User"],
                toDoItemName,
                (DateTime)context.Data["Deadline"],
                toDoList,
                ct);

            var message = "";

            if (toDoList != null)
                message = $"Задача \"{toDoItemName}\" добавлена в список \"{toDoList.Name}\".\n";
            else
                message = $"Задача \"{toDoItemName}\" добавлена в общий список.\n";
            
            return new ScenarioResponse(ScenarioResult.Completed, chat)
            {
                Message = message,
                Keyboard = Helper.keyboardReg
            };
        }

        private async Task<ScenarioResponse> HandleDefaultStep(Chat chat, CancellationToken ct)
        {
            await Task.Delay(1, ct);
            
            return new ScenarioResponse(ScenarioResult.Completed, chat)
            {
                Message = "Неизвестный шаг сценария",
                Keyboard = Helper.keyboardReg
            };
        }
    }
}