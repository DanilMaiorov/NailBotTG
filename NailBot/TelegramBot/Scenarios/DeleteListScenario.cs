using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using NailBot.TelegramBot.Dto;
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
        public DeleteListScenario(IUserService userService, IToDoService toDoService, IToDoListService toDoListService, string toDoItemFolderName) 
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoListService = toDoListService;

            _toDoItemFolderName = toDoItemFolderName;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteList;
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

                case "Approve":
                    return await HandleApproveStep(bot, context, currentUser, currentChat, currentUserInput, ct);

                case "Delete":
                    return await HandleDeleteStep(bot, context, currentUser, currentChat, currentUserInput, ct);

                default:
                    await bot.SendMessage(currentChat, "Неизвестный шаг сценария", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                    break;
            }

            return ScenarioResult.Completed;
        }

        private async Task<ScenarioResult> HandleInitialStep(ITelegramBotClient bot, ScenarioContext context, ToDoUser user, Chat chat, CancellationToken ct)
        {
            context.Data[user.TelegramUserName] = user;

            var lists = await _toDoListService.GetUserLists(user.UserId, ct);

            if (lists.Count > 0)
            {
                await bot.SendMessage(chat, "Выберете список для удаления:", replyMarkup: Helper.GetSelectListKeyboardForDelete(lists), cancellationToken: ct);

                context.CurrentStep = "Approve";

                return ScenarioResult.Transition;
            }
            await bot.SendMessage(chat, "Нет списков для удаления.", replyMarkup: Helper.keyboardReg, cancellationToken: ct);

            return ScenarioResult.Completed;
        }

        private async Task<ScenarioResult> HandleApproveStep(ITelegramBotClient bot, ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            var deleteListGuid = Helper.ParseGuidFromCommand(userInput);

            if (deleteListGuid.HasValue)
            {
                var deleteList = await _toDoListService.Get(deleteListGuid.Value, ct);

                context.Data[user.TelegramUserName] = deleteList;

                await bot.SendMessage(chat, $"Подтверждаете удаление списка {deleteList.Name} и всех его задач?", replyMarkup: Helper.GetApproveDeleteListKeyboard(), cancellationToken: ct);

                context.CurrentStep = "Delete";
            }
            return ScenarioResult.Transition;
        }

        private async Task<ScenarioResult> HandleDeleteStep(ITelegramBotClient bot, ScenarioContext context, ToDoUser user, Chat chat, string userInput, CancellationToken ct)
        {
            if (userInput == "no")
            {
                await bot.SendMessage(chat, $"Удаление отменено", cancellationToken: ct);
            } 
            else
            {
                if (context.Data.TryGetValue(user.TelegramUserName, out var list))
                {
                    if (list is not ToDoList toDoList)
                        throw new ArgumentException("Удаляемый список не является типом списка");

                    //получу все тудушки которые в выбранном списке
                    var items = await _toDoService.GetByUserIdAndList(user.UserId, toDoList.Id, ct);

                    //удлю по очереди с перестройкой индекса
                    if (items.Count > 0)
                    {
                        foreach (var item in items)
                            await _toDoService.Delete(item.Id, ct);
                    }

                    // удаляю папку списка и директории с разделение тудушек по папкам-спискам после удаления всех тудушек выбранного списка - ПОХОЖЕ НА КОСТЫЛЬ
                    var toDoItemsDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), _toDoItemFolderName, user.UserId.ToString(), toDoList.Id.ToString());

                    if (Directory.Exists(toDoItemsDirectoryPath))
                        Directory.Delete(toDoItemsDirectoryPath);

                    //затем удалю папку в todolist директории
                    await _toDoListService.Delete(toDoList.Id, ct);

                    await bot.SendMessage(chat, $"Список {toDoList.Name} удален", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                }
            }
            return ScenarioResult.Completed;
        }
    }
}