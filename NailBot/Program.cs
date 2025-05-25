using Otus.ToDoList.ConsoleBot;
using NailBot.TelegramBot;
using NailBot.Core.Services;
using NailBot.Infrastructure.DataAccess;
using NailBot.Helpers;

namespace NailBot
{
    internal class Program//ЧИСТОВИК
    {
        public static void Main(string[] args)
        {
            ITelegramBotClient _botClient = new ConsoleBotClient();

            var userRepository = new InMemoryUserRepository();
            var toDoRepository = new InMemoryToDoRepository();

            //стартовые значения длин
            //int maxTaskAmount = Helper.GetStartValues("Введите максимально допустимое количество задач");
            //int maxTaskLength = Helper.GetStartValues("Введите максимально допустимую длину задачи");

            //для ускоренного дебага
            int maxTaskAmount = 20;
            int maxTaskLength = 25;

            IUserService _userService = new UserService(userRepository);
            IToDoService _toDoService = new ToDoService(toDoRepository, maxTaskAmount, maxTaskLength);

            IToDoReportService _toDoReportService = new ToDoReportService(toDoRepository);

            //объявлю CancellationTokenSource
            using var cts = new CancellationTokenSource();

            IUpdateHandler _updateHandler = new UpdateHandler(_userService, _toDoService, _toDoReportService, cts.Token);

            if (_updateHandler is UpdateHandler castHandler)
            {
                //подписываюсь на события
                castHandler.OnHandleUpdateStarted += castHandler.HandleStart;
                castHandler.OnHandleUpdateCompleted += castHandler.HandleComplete;

                try
                {
                    _botClient.StartReceiving(_updateHandler, cts.Token);
                    Console.WriteLine("asd");
                }
                finally
                {
                    //отписываюсь от событий
                    castHandler.OnHandleUpdateStarted -= castHandler.HandleStart;
                    castHandler.OnHandleUpdateCompleted -= castHandler.HandleComplete;
                }
            }
        }
    }
}
