using NailBot.TelegramBot;
using NailBot.Core.Services;
using NailBot.Infrastructure.DataAccess;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using NailBot.TelegramBot.Scenarios;
using System.Globalization;

namespace NailBot
{
    internal class Program//ЧИСТОВИК
    {
        //объявлю имена папок через константы
        //имя папки для ToDoItem
        private const string toDoItemfolderName = "ToDoItemFolder";
        //имя папки для User
        private const string userfolderName = "UserFolder";
        //имя папки для списков
        private const string toDoListfolderName = "ToDoListFolder";

        public async static Task Main(string[] args)
        {
            //string token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User);
            string token = "7512417913:AAHnoeWdDKNOyTuF0DMHpPVdO95imk0xMgw";

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

            //создам класс FileToDoRepository
            var fileToDoRepository = new FileToDoRepository(toDoItemfolderName);
            
            //создам класс FileUserRepository
            var fileUserRepository = new FileUserRepository(userfolderName);


            IUserService _userService = new UserService(fileUserRepository);
            IToDoService _toDoService = new ToDoService(fileToDoRepository, maxTaskAmount, maxTaskLength);

            IToDoReportService _toDoReportService = new ToDoReportService(fileToDoRepository);



            //логика списка задач
            var fileToDoListRepository = new FileToDoListRepository(toDoListfolderName, toDoItemfolderName);
            IToDoListService _toDoListService = new ToDoListService(fileToDoListRepository);


            //логика сценариев
            IScenarioContextRepository contextRepository = new InMemoryScenarioContextRepository();
            var scenarios = new List<IScenario>
            {
                new AddTaskScenario(_userService, _toDoService, _toDoListService),
                new AddListScenario(_userService, _toDoListService)
            };

            IUpdateHandler _updateHandler = new UpdateHandler(_userService, _toDoService, _toDoReportService, scenarios, contextRepository, _toDoListService, cts.Token);

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
