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
            var botClient = new ConsoleBotClient();

            var userRepository = new InMemoryUserRepository();
            var toDoRepository = new InMemoryToDoRepository();

            //стартовые значения длин
            int maxTaskAmount = Helper.GetStartValues("Введите максимально допустимое количество задач");
            int maxTaskLength = Helper.GetStartValues("Введите максимально допустимую длину задачи");

            //для ускоренного дебага
            //int maxTaskAmount = 20;
            //int maxTaskLength = 25;

            var _userService = new UserService(userRepository);
            var _toDoService = new ToDoService(toDoRepository , maxTaskAmount, maxTaskLength);

            var _toDoReportService = new ToDoReportService(toDoRepository);

            //объявлю CancellationTokenSource
            var cts = new CancellationTokenSource();

            Init StartBot = new Init(botClient, _userService, _toDoService, _toDoReportService, cts);
            
                try
                {
                    //подписываюсь на события
                    //StartBot.BotUpdateHandler.OnHandleUpdateStarted += StartBot.BotUpdateHandler.HandleStart;
                    //StartBot.BotUpdateHandler.OnHandleUpdateCompleted += StartBot.BotUpdateHandler.HandleComplete;

                    //стартую бота
                    StartBot.Start();

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла непредвиденная ошибка");
                    Console.WriteLine($"Стек трейс основного исключения: {ex.StackTrace}\n");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}\n");
                        Console.WriteLine($"Источник внутреннего исключения: {ex.InnerException.StackTrace}\n");
                    }
                }
                finally
                {               
                    //отписываюсь от событий
                    //StartBot.BotUpdateHandler.OnHandleUpdateStarted -= StartBot.BotUpdateHandler.HandleStart;
                    //StartBot.BotUpdateHandler.OnHandleUpdateCompleted -= StartBot.BotUpdateHandler.HandleComplete;

                    //ториожу бота,, прощаюсь
                    StartBot.Stop();
                }

                
        }
    }
}

