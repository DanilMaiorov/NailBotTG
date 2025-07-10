using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bots;
using Telegram.Bots.Http;

namespace NailBot.TelegramBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        private readonly IToDoListService _toDoListService;

        private string _taskName;
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
            Message message;
            ToDoUser currentUser;
            Chat currentChat;
            string currentUserInput;

            if (update.Message != null)
            {
                message = update.Message;
                currentUser = await _userService.GetUser(message.From.Id, ct);
                currentChat = message.Chat;
                currentUserInput = message.Text?.Trim();
            }
            else if (update.CallbackQuery != null)
            {
                message = update.CallbackQuery.Message;
                currentUser = await _userService.GetUser(update.CallbackQuery.From.Id, ct);
                currentChat = update.CallbackQuery.Message.Chat;
                currentUserInput = update.CallbackQuery.Data?.Trim();
            }
            else
            {
                //await OnUnknown(update);
                return ScenarioResult.Completed;
            }

            switch (context.CurrentStep)
            {
                case null:
                    return await HandleInitialStep(botClient, context, currentUser, currentChat, ct);

                case "Name":
                    return await HandleNameStep(botClient, context, currentUser, currentChat, currentUserInput, ct);

                case "Deadline":
                    return await HandleDeadlineStep(botClient, context, currentUser, currentChat, currentUserInput, ct);

                case "List":
                    return await HandleChooseListStep(botClient, context, currentUser, currentChat, currentUserInput, ct);

                default:
                    await botClient.SendMessage(currentChat, "Неизвестный шаг сценария", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
             }
            return ScenarioResult.Completed;
        }


        private async Task<ScenarioResult> HandleInitialStep(ITelegramBotClient botClient, ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            context.Data[user.TelegramUserName] = user;

            await botClient.SendMessage(chat, "Введите название задачи:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);

            context.CurrentStep = "Name";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> HandleNameStep(ITelegramBotClient botClient, ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            var tasks = await _toDoService.GetAllByUserId(user.UserId, ct);

            Helper.CheckDuplicate(userInput, tasks);

            //_taskName = userInput;

            context.Data[user.TelegramUserName] = new ToDoItem()
            {
                Name = userInput,
            };


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

            if (!context.Data.TryGetValue(user.TelegramUserName, out var toDoItemObj))
                throw new InvalidOperationException("Пользователь не найден в контексте");

            var obj = (ToDoItem)toDoItemObj;

            obj.Deadline = deadline;

            //await _toDoService.Add((ToDoUser)userObj, _taskName, deadline, null, ct);



            context.CurrentStep = "List";

            var lists = await _toDoListService.GetUserLists(user.UserId, ct);

            await botClient.SendMessage(chat, "Выберите список", replyMarkup: Helper.GetSelectListKeyboard(lists), cancellationToken: ct);

            return ScenarioResult.Transition;
        }


        private async Task<ScenarioResult> HandleChooseListStep(ITelegramBotClient botClient, ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            //_toDoService.GetByUserIdAndList();
            var lists = await _toDoListService.GetUserLists(user.UserId, ct);

            //var list = lists.Where(x => userInput == x.Name).ToList()[0];

            //_toDoService.GetByUserIdAndList();

            //if (!Helper.TryParseUserDate(userInput, out DateTime deadline))
            //{
            //    //Helper.GetSelectListKeyboard();
            //    return ScenarioResult.Transition;
            //}

            if (!context.Data.TryGetValue(user.TelegramUserName, out var toDoItemObj))
                throw new InvalidOperationException("Пользователь не найден в контексте");

            var obj = (ToDoItem)toDoItemObj;


            await _toDoService.Add(user, obj.Name, obj.Deadline, new ToDoList(), ct);
            //ТУТ ПОКА НЕ ПОНИМАЮ КАК ДОЛЖНО РАБОТАТЬ
            //await _toDoService.Add((ToDoUser)userObj, _taskName, deadline, null, ct);

            //await botClient.SendMessage(chat, $"Задача \"{_taskName}\" добавлена в список задач.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);

            //_taskName = string.Empty;


            await botClient.SendMessage(chat, $"Задача \"{_taskName}\" добавлена в список задач.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);

            return ScenarioResult.Completed;
        }
    }
}