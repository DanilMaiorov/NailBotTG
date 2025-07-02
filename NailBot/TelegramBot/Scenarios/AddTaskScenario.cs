using NailBot.Core.Entities;
using NailBot.Core.Enums;
using NailBot.Core.Services;
using NailBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bots.Http;

namespace NailBot.TelegramBot.Scenarios
{
    public class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var currentUser = await _userService.GetUser(update.Message.From.Id, ct);
            switch (context.CurrentStep)
            {
                case null:
                    

                    context.Data[currentUser.TelegramUserName] = currentUser;

                    bot.SendMessage(update.Message.Chat, "Введите название задачи:", cancellationToken: ct);
                    context.CurrentStep = "Name";
                    return await Task.FromResult(ScenarioResult.Transition);

                case "Name":

                    if (context.Data.TryGetValue(currentUser.TelegramUserName, out object user))
                    {
                        _toDoService.Add((ToDoUser)user, update.Message.Text, ct);

                        await bot.SendMessage(update.Message.Chat, $"Задача \"{update.Message.Text}\" добавлена в список задач.\n", replyMarkup: Helper.keyboardReg, cancellationToken: ct);
                        return await Task.FromResult(ScenarioResult.Completed);
                    }
                    break;
            }

            throw new NotImplementedException();
        }

        public AddTaskScenario(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }
    }
}