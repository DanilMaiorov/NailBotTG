using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailBot.TelegramBot.Scenarios
{
    public class DeleteTaskScenario : IScenario
    {
        private readonly IToDoService _toDoService;

        public DeleteTaskScenario(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            //верну выполненный сценарий если придёт какая-то левая инфа
            if (update.Message == null && update.CallbackQuery == null)
                return ScenarioResult.Completed;

            (Chat? currentChat, string? currentUserInput, int currentMessageId, _) = await Helper.HandleMessageAsyncGetData(update, context, ct);

            switch (context.CurrentStep)
            {
                case null:
                    return await HandleApproveStep(bot, context, currentChat, currentUserInput, ct);

                case "Delete":
                    return await HandleDeleteStep(bot, context, currentChat, currentMessageId, currentUserInput, ct);

                default:
                    await bot.SendMessage(currentChat, "Неизвестный шаг сценария", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
            }

            return ScenarioResult.Completed;
        }

        private async Task<ScenarioResult> HandleApproveStep(
            ITelegramBotClient bot, 
            ScenarioContext context,
            Chat chat, 
            string userInput, 
            CancellationToken ct)
        {
            var toDoItemGuid = Helper.ParseGuidFromCommand(userInput);

            if (toDoItemGuid.HasValue)
            {
                var deleteToDoItem = await _toDoService.Get(toDoItemGuid.Value, ct);

                context.Data["Item"] = deleteToDoItem;

                await bot.SendMessage(chat, $"Подтверждаете удаление задачи {deleteToDoItem.Name}?", replyMarkup: Helper.GetApproveDeleteToDoListAndToDoItemKeyboard(), cancellationToken: ct);

                context.CurrentStep = "Delete";
            }
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> HandleDeleteStep(
            ITelegramBotClient bot, 
            ScenarioContext context, 
            Chat chat, 
            int messageId,
            string userInput, 
            CancellationToken ct)
        {
            if (userInput == "no")
            {
                await bot.SendMessage(chat, $"Удаление отменено", cancellationToken: ct);
            }
            else
            {
                if (context.Data.TryGetValue("Item", out var currentTask))
                {
                    if (currentTask is not ToDoItem toDoItem)
                        throw new ArgumentException("Удаляемый список не является типом задачи");

                    await _toDoService.Delete(toDoItem.Id, ct);

                    await bot.EditMessageText(
                        chat,
                        messageId - 1,
                        $"{toDoItem.Name}: \n\nСрок выполнения: {toDoItem.Deadline}\nВремя создания: {toDoItem.CreatedAt}",
                        replyMarkup: default,
                        cancellationToken: ct);

                    await bot.EditMessageText(
                        chat,
                        messageId,
                        $"Подтверждаете удаление задачи {toDoItem.Name}?",
                        replyMarkup: default,
                        cancellationToken: ct);

                    await bot.SendMessage(chat, $"Задача {toDoItem.Name} удалена", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                }
            }
            return ScenarioResult.Completed;
        }
    }
}