
using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NailBot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //переменная проверки выхода из программы
            bool isFinish = false;

            while (!isFinish)
            {
                try
                {
                    Init.Start(Init.startTaskAmount);
                    isFinish = true; 
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("Введено некорректное количество задач");
                    Console.WriteLine($"Тип ошибки: {ex.Message}");
                    Console.WriteLine($"Стек трейс: {ex.StackTrace}");
                    Console.WriteLine($"Где произошла: {ex.InnerException}");
                }
                catch (TaskCountLimitException ex)
                {
                    Console.WriteLine($"Тип ошибки: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Произошла непредвиденная ошибка");
                    Console.WriteLine($"Тип ошибки: {ex.Message}");
                    Console.WriteLine($"Стек трейс: {ex.StackTrace}");
                    Console.WriteLine($"Где произошла: {ex.InnerException}");
                }
            }

            //Сделать рекурсией

            //DescriptionBot.PrintDescription();
        }
    }
}

