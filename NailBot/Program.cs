
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
                    try
                    {
                        Init.Start(Init.maxTaskAmount, Init.maxTaskLenght);
                        isFinish = true;
                    }
                    catch (ArgumentException ex)
                    {
                        //выбрасываю новую ошибку, а во внешнем catch обращаюсь к InnerException чтобы отследить путь откуда выброшена ошибка
                        throw new Exception("Произошла непредвиденная ошибка", ex);
                        //если выбросить просто через throw, то InnerException будет null
                        //throw;
                    }
                    catch (TaskCountLimitException ex)
                    {
                        throw new Exception("Произошла непредвиденная ошибка", ex);
                    }
                    catch (TaskLengthLimitException ex)
                    {
                        throw new Exception("Произошла непредвиденная ошибка", ex);
                    }
                    catch (DuplicateTaskException ex)
                    {
                        throw new Exception("Произошла непредвиденная ошибка", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Произошла непредвиденная ошибка", ex);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Основное исключение: {ex.Message}\n");
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

