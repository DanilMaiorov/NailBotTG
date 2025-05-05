
using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace NailBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //создаю экземпляр бота
            var botClient = new ConsoleBotClient();

            //объявляю переменную типа интефрейса IUserService _userService
            var _userService = new UserService();
            var _toDoService = new ToDoService();

            Init StartBot = new Init(botClient, _userService, _toDoService);
            
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

