using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace NailBot
{
    public class Init
    {
        //создаю экземпляр бота
        public static ConsoleBotClient botClient = new ConsoleBotClient();
        public static ITelegramBotClient telegramBotClient = new ConsoleBotClient();

        //создаю экземпляр IUserService
        public static IUserService iuserService = new UserService();

        //создаю экземпляр IToDoService
        public static IToDoService itoDoService = new ToDoService();

        //создаю экземпляр объекта хендлера
        internal UpdateHandler handler = new UpdateHandler(iuserService, itoDoService);

        //создаю экземпляр объекта апдейт
        public Update update = new Update();

        public void Start()
        {
            //запуск бота
            botClient.StartReceiving(handler);
        }

        public static void Stop()
        {    
            //botClient.SendMessage(update.Message.Chat, $"До свидания, {userName}! До новых встреч!");
            Console.ReadKey();
            return;
        }
    }
}
