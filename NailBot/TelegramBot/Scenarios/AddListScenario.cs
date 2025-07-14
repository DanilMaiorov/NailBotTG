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
                    return await HandleInitialStep(bot, context, currentUser, currentChat, ct);

                case "Name":
                    return await HandleNameStep(bot, context, currentUser, currentChat, currentUserInput, ct);

                default:
                    await bot.SendMessage(currentChat, "Неизвестный шаг сценария", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
            }
            await Task.Delay(1);

            return ScenarioResult.Completed;
        }

        private async Task<ScenarioResult> HandleInitialStep(ITelegramBotClient bot, ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            context.Data[user.TelegramUserName] = user;

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
