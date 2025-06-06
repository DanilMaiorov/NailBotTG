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



            //удаляю папку с ToDoItem и ToDoUser для проверки работоспособности

            var currentDirectory = Directory.GetCurrentDirectory();

            Console.WriteLine();

            var p = Path.Combine(currentDirectory, "..", "..", "..", "ToDoItemRepository");

            Directory.Delete(p, true);

            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }






            //проверку на наличие свободного места делать не буду в этом проекте}
            Console.WriteLine();


            Console.WriteLine("cur dir {0}", Environment.CurrentDirectory);
            Console.WriteLine("sys dir {0}", Environment.SystemDirectory);
            Console.WriteLine("appdata dir {0}", Environment.SpecialFolder.ApplicationData);
            Console.WriteLine("personal dir {0}", Environment.SpecialFolder.Personal);
            Console.WriteLine("Ниже папки");
            Console.WriteLine("appdata dir {0}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            Console.WriteLine("personal dir {0}", Environment.GetFolderPath(Environment.SpecialFolder.Personal));

            //проверку на наличие свободного места делать не буду в этом проекте}
            Console.WriteLine();


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
