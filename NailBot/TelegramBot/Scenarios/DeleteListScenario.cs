using Microsoft.Extensions.Options;
using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using NailBot.Options;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailBot.TelegramBot.Scenarios
{
    public class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoListService _toDoListService;

        //добавлю ещё имя папки с тудушками в конструктор
        private readonly string _toDoItemFolderName;
        public DeleteListScenario(IUserService userService, IToDoService toDoService, IToDoListService toDoListService, IOptions<FolderOptions> options) 
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoListService = toDoListService;

            _toDoItemFolderName = options.Value.ToDoItemFolderName;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteList;
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

                case "Approve":
                    return await HandleApproveStep(context, currentUser, currentChat, currentUserInput, ct);

                case "Delete":
                    return await HandleDeleteStep(context, currentUser, currentChat, currentUserInput, ct);

                default:
                    return await HandleDefaultStep(currentChat, ct);
            }
        }

        private async Task<ScenarioResponse> HandleInitialStep(ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            //context.Data[user.TelegramUserName] = user;
            context.Data["User"] = user;

            var lists = await _toDoListService.GetUserLists(user.UserId, ct);

            if (lists.Count > 0)
            {
                context.CurrentStep = "Approve";
                
                return new ScenarioResponse(ScenarioResult.Transition, chat)
                {
                    Message = "Выберете список для удаления:",
                    Keyboard = Helper.GetSelectListKeyboardForDelete(lists)
                };
            }

            return new ScenarioResponse(ScenarioResult.Completed, chat)
            {
                Message = "Нет списков для удаления.",
                Keyboard = Helper.keyboardReg
            };
        }

        private async Task<ScenarioResponse> HandleApproveStep(ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            var message = "";
            
            var deleteListGuid = Helper.ParseGuidFromCommand(userInput);

            if (deleteListGuid.HasValue)
            {
                var deleteList = await _toDoListService.Get(deleteListGuid.Value, ct);

                //context.Data[user.TelegramUserName] = deleteList;
                context.Data["User"] = deleteList;

                message = $"Подтверждаете удаление списка {deleteList.Name} и всех его задач?";

                context.CurrentStep = "Delete";
            }
            
            return new ScenarioResponse(ScenarioResult.Transition, chat)
            {
                Message = message,
                Keyboard = Helper.GetApproveDeleteToDoListAndToDoItemKeyboard()
            };
        }

        private async Task<ScenarioResponse> HandleDeleteStep(ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            var message = "";
            
            if (userInput == "no")
            {
                message = "Удаление отменено";
            } 
            else
            {
                if (context.Data.TryGetValue("User", out var list))
                {
                    if (list is not ToDoList toDoList)
                        throw new ArgumentException("Удаляемый список не является типом списка");

                    //получу все тудушки которые в выбранном списке
                    var items = await _toDoService.GetByUserIdAndList(user.UserId, toDoList.Id, ct);

                    //удлю по очереди с перестройкой индекса
                    if (items.Count > 0)
                        await Task.WhenAll(items.Select(item => _toDoService.Delete(item.Id, ct)));
                    
                    // удаляю папку списка и директории с разделение тудушек по папкам-спискам после удаления всех тудушек выбранного списка - ПОХОЖЕ НА КОСТЫЛЬ
                    var toDoItemsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), _toDoItemFolderName, user.UserId.ToString(), toDoList.Id.ToString());

                    if (Directory.Exists(toDoItemsDirectoryPath))
                        Directory.Delete(toDoItemsDirectoryPath);

                    //затем удалю папку в todolist директории
                    await _toDoListService.Delete(toDoList.Id, ct);
                    
                    message = $"Список {toDoList.Name} удален";
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