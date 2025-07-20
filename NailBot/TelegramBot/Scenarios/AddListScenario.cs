using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailBot.TelegramBot.Scenarios
{
    public class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        public AddListScenario(IUserService userService, IToDoListService toDoListService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddList;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            //верну выполненный сценарий если придёт какая-то левая инфа
            if (update.Message == null && update.CallbackQuery == null)
                return ScenarioResult.Completed;

            (Chat? currentChat, string? currentUserInput, ToDoUser? currentUser) = await Helper.HandleMessageAsyncGetData(update, context, _userService, ct);

            switch (context.CurrentStep)
            {
                case null:
                    return await HandleInitialStep(bot, context, currentUser, currentChat, ct);

                case "Name":
                    return await HandleNameStep(bot, context, currentUser, currentChat, currentUserInput, ct);

                default:
                    await bot.SendMessage(currentChat, "Неизвестный шаг сценария", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
            }

            return ScenarioResult.Completed;
        }

        private async Task<ScenarioResult> HandleInitialStep(ITelegramBotClient bot, ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            context.Data["User"] = user;

            await bot.SendMessage(chat, "Введите название списка:", replyMarkup: Helper.keyboardCancel, cancellationToken: ct);

            context.CurrentStep = "Name";

            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> HandleNameStep(ITelegramBotClient bot, ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            await _toDoListService.Add(user, userInput, ct);

            await bot.SendMessage(chat, $"Список {userInput} добавлен", replyMarkup: Helper.keyboardReg, cancellationToken: ct);

            return ScenarioResult.Completed;
        }
    }
}
