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
            //создаю экземпляр бота
            var botClient = new ConsoleBotClient();

            //экземпляры репозиториев
            var userRepository = new InMemoryUserRepository();
            var toDoRepository = new InMemoryToDoRepository();

            //стартовые значения длин
            int maxTaskAmount = Helper.GetStartValues("Введите максимально допустимое количество задач");
            int maxTaskLength = Helper.GetStartValues("Введите максимально допустимую длину задачи");

            //объявляю переменную типа интефрейса IUserService _userService
            var _userService = new UserService(userRepository);
            var _toDoService = new ToDoService(toDoRepository , maxTaskAmount, maxTaskLength);

            var _toDoReportService = new ToDoReportService(toDoRepository);

            Init StartBot = new Init(botClient, _userService, _toDoService, _toDoReportService);
            
            //переменная проверки выхода из программы
            bool isFinish = false;

            while (!isFinish)
            {
                try
                {
                    StartBot.Start();
                    isFinish = true;
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
            }
            Init.Stop();
        }
    }
}

