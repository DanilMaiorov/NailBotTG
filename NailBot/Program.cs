
using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NailBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //здороваемся только 1 раз при запуске бота
            Console.WriteLine($"Привет! Это Todo List Bot! Введите команду для начала работы или выхода из бота.\n");

            //переменная проверки выхода из программы
            bool isFinish = false;

            while (!isFinish)
            {
                try
                {
                    Init.Start(Init.maxTaskAmount, Init.maxTaskLenght);
                    isFinish = true;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (TaskCountLimitException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (TaskLengthLimitException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (DuplicateTaskException ex)
                {
                    Console.WriteLine(ex.Message);
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
        }
    }
}

