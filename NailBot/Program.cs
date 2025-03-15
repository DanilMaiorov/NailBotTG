
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

            //DescriptionBot.PrintDescription();

            // не совсем до конца понял по количеству try catch и как будто бы переборщил с ними))) 
            // повешал глобальный try catch внутри которого ещё try catch которые отлавливают исключения из задания
            // и выбрасывают ещё одно, чтобы прочитать InnerException, всё время получал null, потому так обвешал

            // не совсем понял каким образом отловить Exception и вывести InnerException потому что во всех случаях ввода предусматриваются те или иные кастомные Exception
            // по заданию там нужно выводить только сообщение об ошибке, без innerException
        }
    }
}

