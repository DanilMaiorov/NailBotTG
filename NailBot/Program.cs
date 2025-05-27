using NailBot.TelegramBot;
using NailBot.Core.Services;
using NailBot.Infrastructure.DataAccess;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace NailBot
{
    internal class Program//ЧИСТОВИК
    {
        public async static Task Main(string[] args)
        {
            string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User);

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Bot token not found. Please set the TELEGRAM_BOT_TOKEN environment variable.");
                return;
            }

            var botClient = new TelegramBotClient(token);

            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = [] // принимаю любой тип
            };

            //объявлю CancellationTokenSource
            using var cts = new CancellationTokenSource();

            //стартовые значения длин
            //int maxTaskAmount = Helper.GetStartValues("Введите максимально допустимое количество задач");
            //int maxTaskLength = Helper.GetStartValues("Введите максимально допустимую длину задачи");

            //для ускоренного дебага
            int maxTaskAmount = 20;
            int maxTaskLength = 25;

            var userRepository = new InMemoryUserRepository();
            var toDoRepository = new InMemoryToDoRepository();

            IUserService _userService = new UserService(userRepository);
            IToDoService _toDoService = new ToDoService(toDoRepository, maxTaskAmount, maxTaskLength);
            IToDoReportService _toDoReportService = new ToDoReportService(toDoRepository);

            IUpdateHandler _updateHandler = new UpdateHandler(_userService, _toDoService, _toDoReportService, cts.Token);

            if (_updateHandler is UpdateHandler castHandler)
            {
                //подписываюсь на события
                castHandler.OnHandleUpdateStarted += castHandler.HandleStart;
                castHandler.OnHandleUpdateCompleted += castHandler.HandleComplete;

                try
                {
                    botClient.StartReceiving(
                        _updateHandler,
                        receiverOptions: receiverOptions,
                        cancellationToken: cts.Token
                    );

                    //запускаю цикл, которые будет работать пока не нажму А
                    while (true)
                    {
                        Console.WriteLine("Нажми A и Ввод для остановки и выхода из бота");
                        var s = Console.ReadLine();

                        if (s?.ToUpper() == "A")
                        {
                            cts.Cancel();
                            Console.WriteLine("Бот остановлен");
                            break;
                        }
                        var me = await botClient.GetMe();
                        Console.WriteLine($"{me.FirstName} запущен!");
                    }
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
