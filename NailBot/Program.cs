
using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NailBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //переменная проверки выхода из программы
            bool isFinish = false;

            Init StartBot = new Init();

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

