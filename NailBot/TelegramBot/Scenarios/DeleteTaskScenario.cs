using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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

        public async Task<ScenarioResponse> HandleMessageAsync(ScenarioContext context, Chat chat, Update update, CancellationToken ct)
        {
            //верну выполненный сценарий если придёт какая-то левая инфа
            if (update.Message == null && update.CallbackQuery == null)
                return new ScenarioResponse(ScenarioResult.Completed, chat)
                {
                    Message = "Это ToDoList Bot", //тут докрутить
                    Keyboard = Helper.keyboardReg
                };

            (Chat? currentChat, string? currentUserInput, int currentMessageId, _) = await Helper.HandleMessageAsyncGetData(update, context, ct);

            switch (context.CurrentStep)
            {
                case null:
                    return await HandleApproveStep(context, currentChat, currentUserInput, ct);

                case "Delete":
                    return await HandleDeleteStep(context, currentChat, currentMessageId, currentUserInput, ct);

                default:
                    return await HandleDefaultStep(currentChat, ct);
            }
        }

        private async Task<ScenarioResponse> HandleApproveStep(
            ScenarioContext context,
            Chat chat, 
            string userInput, 
            CancellationToken ct)
        {
            var message = "";
            
            var toDoItemGuid = Helper.ParseGuidFromCommand(userInput);

            if (toDoItemGuid.HasValue)
            {
                var deleteToDoItem = await _toDoService.Get(toDoItemGuid.Value, ct);

                context.Data["Item"] = deleteToDoItem;
                
                message = $"Подтверждаете удаление задачи {deleteToDoItem.Name}?";

                context.CurrentStep = "Delete";
            }
            
            return new ScenarioResponse(ScenarioResult.Transition, chat)
            {
                Message = message,
                Keyboard = Helper.GetApproveDeleteToDoListAndToDoItemKeyboard()
            };
        }

        private async Task<ScenarioResponse> HandleDeleteStep(
            ScenarioContext context, 
            Chat chat, 
            int messageId,
            string userInput, 
            CancellationToken ct)
        {
            var message = "";
            
            if (userInput == "no")
            {
                message = "Удаление отменено";
            }
            else
            {
                if (context.Data.TryGetValue("Item", out var currentTask))
                {
                    if (currentTask is not ToDoItem toDoItem)
                        throw new ArgumentException("Удаляемый список не является типом задачи");

                    await _toDoService.Delete(toDoItem.Id, ct);

                    // await bot.EditMessageText(
                    //     chat,
                    //     messageId - 1,
                    //     $"{toDoItem.Name}: \n\nСрок выполнения: {toDoItem.Deadline}\nВремя создания: {toDoItem.CreatedAt}",
                    //     replyMarkup: default,
                    //     cancellationToken: ct);
                    //
                    // await bot.EditMessageText(
                    //     chat,
                    //     messageId,
                    //     $"Подтверждаете удаление задачи {toDoItem.Name}?",
                    //     replyMarkup: default,
                    //     cancellationToken: ct);
                    //
                    // await bot.SendMessage(chat, $"Задача {toDoItem.Name} удалена", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    
                    message = $"Задача {toDoItem.Name} удалена";
                    
                    return new ScenarioResponse(ScenarioResult.Completed, chat)
                    {
                        Message = message,
                        Keyboard = Helper.keyboardReg,
                        EditMessages = new List<(int, string, InlineKeyboardMarkup)>
                        {
                            (messageId - 1, $"{toDoItem.Name}: \n\nСрок выполнения: {toDoItem.Deadline}\nВремя создания: {toDoItem.CreatedAt}", default),
                            (messageId, $"Подтверждаете удаление задачи {toDoItem.Name}?", default)
                        },
                        IsEdit = true
                    };
                }
            }
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