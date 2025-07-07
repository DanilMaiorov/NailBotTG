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

        private string _taskName;
        public AddTaskScenario(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var message = update.Message;
            var currentUser = await _userService.GetUser(message.From.Id, ct);
            var currentChat = message.Chat;
            var currentUserInput = message.Text?.Trim();

            switch (context.CurrentStep)
            {
                case null:
                    return await HandleInitialStep(bot, context, currentUser, currentChat, ct);

                case "Name":
                    return await HandleNameStep(bot, context, currentUser, currentChat, currentUserInput, ct);

                case "Deadline":
                    return await HandleDeadlineStep(bot, context, currentUser, currentChat, currentUserInput, ct);

                default:
                    await bot.SendMessage(currentChat, "Неизвестный шаг сценария", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
             }
            return ScenarioResult.Completed;
        }


        private async Task<ScenarioResult> HandleInitialStep(ITelegramBotClient bot, ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            context.Data[user.TelegramUserName] = user;

            await bot.SendMessage(chat, "Введите название задачи:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);

            context.CurrentStep = "Name";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> HandleNameStep(ITelegramBotClient bot, ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            var tasks = await _toDoService.GetAllByUserId(user.UserId, ct);

            Helper.CheckDuplicate(userInput, tasks);

            _taskName = userInput;

            await bot.SendMessage(chat, "Введите дедлайн задачи в формате dd.MM.yyyy:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);

            context.CurrentStep = "Deadline";
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> HandleDeadlineStep(ITelegramBotClient bot, ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            if (!Helper.TryParseUserDate(userInput, out DateTime deadline))
            {
                await bot.SendMessage(chat, "Неверный формат даты. Попробуйте ещё раз в формате dd.MM.yyyy:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);
                return ScenarioResult.Transition;
            }

            if (!context.Data.TryGetValue(user.TelegramUserName, out var userObj))
                throw new InvalidOperationException("Пользователь не найден в контексте");




            //await _toDoService.Add((ToDoUser)userObj, _taskName, deadline, ct);
            //ТУТ ПОКА НЕ ПОНИМАЮ КАК ДОЛЖНО РАБОТАТ
            await _toDoService.Add((ToDoUser)userObj, _taskName, deadline, null, ct);

            await bot.SendMessage(chat, $"Задача \"{_taskName}\" добавлена в список задач.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);

            _taskName = string.Empty;
            return ScenarioResult.Completed;
        }
    }
}