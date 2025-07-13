using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailBot.TelegramBot.Scenarios
{
    public class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoListService _toDoListService;
        public DeleteListScenario(IUserService userService, IToDoService toDoService, IToDoListService toDoListService) 
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteList;
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

                //case "Deadline":
                //    return await HandleDeadlineStep(bot, context, currentUser, currentChat, currentUserInput, ct);

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

            var lists = await _toDoListService.GetUserLists(user.UserId, ct);
            await bot.SendMessage(chat, "Выберете список для удаления:", replyMarkup: Helper.GetSelectListKeyboardForShow(lists));

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


//Добавление и удаление списка

//case null
//Получить ToDoUser и сохранить его в ScenarioContext.Data.
//Отправить пользователю сообщение "Выберете список для удаления:" с Inline кнопками. callbackData = ToDoListCallbackDto.ToString(). Action = "deletelist"
//Обновить ScenarioContext.CurrentStep на "Approve"
//case "Approve"
//Получить ToDoList и сохранить его в ScenarioContext.Data.
//Отправить пользователю сообщение "Подтверждаете удаление списка {toDoList.Name} и всех его задач" с Inline кнопками: WithCallbackData("✅Да", "yes"), WithCallbackData("❌Нет", "no")
//Обновить ScenarioContext.CurrentStep на "Delete"
//case "Delete"
//ЕСЛИ update.CallbackQuery.Data равна
//"yes" ТО удалить все задачи по ToDoUser и ToDoList. Удалить ToDoList.
//"no" ТО отправить сообщение "Удаление отменено".
//Вернуть ScenarioResult.Completed.
//При нажатии на кнопку "❌Удалить" должен запускаться сценарий DeleteListScenario