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

        public async Task<ScenarioResponse> HandleMessageAsync(ScenarioContext context, Chat chat, Update update, CancellationToken ct)
        {
            //верну выполненный сценарий если придёт какая-то левая инфа
            if (update.Message == null && update.CallbackQuery == null)
               return new ScenarioResponse(ScenarioResult.Completed, chat)
               {
                   Message = "Это ToDoList Bot", //тут докрутить
                   Keyboard = Helper.keyboardReg
               };

            (Chat? currentChat, string? currentUserInput, int messageId, ToDoUser? currentUser) = await Helper.HandleMessageAsyncGetData(update, context, ct, _userService);

            switch (context.CurrentStep)
            {
                case null:
                    return await HandleInitialStep(context, currentUser, currentChat, ct);

                case "Name":
                    return await HandleNameStep(currentUser, currentChat, currentUserInput, ct);

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
                Message = "Введите название списка:",
                Keyboard = Helper.keyboardCancel
            };
        }

        private async Task<ScenarioResponse> HandleNameStep(ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            await _toDoListService.Add(user, userInput, ct);
            
            return new ScenarioResponse(ScenarioResult.Completed, chat)
            {
                Message = $"Список {userInput} добавлен",
                Keyboard = Helper.keyboardReg
            };
        }
        
        private async Task<ScenarioResponse> HandleDefaultStep(Chat chat, CancellationToken ct)
        {
            return new ScenarioResponse(ScenarioResult.Completed, chat)
            {
                Message = "Неизвестный шаг сценария",
                Keyboard = Helper.keyboardReg
            };
        }
    }
}
