using NailBot.Core.Services;
using Otus.ToDoList.ConsoleBot;

namespace NailBot.TelegramBot
{
    public class Init
    {
        private readonly ITelegramBotClient _botClient;

        private readonly IUpdateHandler _updateHandler;

        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        private readonly IToDoReportService _toDoReportService;

        public Init(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;

            _toDoReportService = toDoReportService;

            _updateHandler = new UpdateHandler(userService, toDoService, toDoReportService);
        }

        public void Start()
        {
            //запуск бота
            _botClient.StartReceiving(_updateHandler);
        }

        public static void Stop()
        {
            //botClient.SendMessage(update.Message.Chat, $"До свидания, {userName}! До новых встреч!");
            Console.ReadKey();
            return;
        }
    }
}
