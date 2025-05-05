using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace NailBot
{
    public class Init
    {
        private readonly ITelegramBotClient _botClient;

        private readonly IUpdateHandler _updateHandler;

        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        public Init(ITelegramBotClient botClient, IUserService userService, IToDoService toDoService)
        {
            _botClient = botClient;
            _userService = userService;
            _toDoService = toDoService;

            _updateHandler = new UpdateHandler(userService, toDoService);
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
