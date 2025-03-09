using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NailBot
{
    internal static class CommandsList
    {
        //метод команды Help
        internal static void ShowHelp(string userName, bool availableEcho)
        {
            if (availableEcho)
            {
                Console.WriteLine($"{userName}, это NailBot - телеграм бот для записи на ноготочки.\n" +
                $"Введя команду /start бот предложит тебе ввести имя\n" +
                $"Введя команду /help ты получишь справку о командах\n" +
                $"Введя команду /echo и какой либо текст после - этот текст останется на консоли\n" +
                $"Введя команду /addtask ты сможешь добавлять задачи в список задач\n" +
                $"Введя команду /showtasks ты сможешь увидеть список добавленных задач в список задач\n" +
                $"Введя команду /removetask ты сможешь удалять задачи из списка задач\n" +
                $"Введя команду /info ты получишь информацию о версии программы\n" +
                $"Введя команду /exit бот попрощается и завершит работу\n");
            }
            else
            {
                Console.WriteLine($"{userName}, это NailBot - телеграм бот для записи на ноготочки.\n" +
                $"Введя команду /start бот предложит тебе ввести имя\n" +
                $"Введя команду /help ты получишь справку о командах\n" +
                $"Введя команду /addtask ты сможешь добавлять задачи в список задач\n" +
                $"Введя команду /showtasks ты сможешь увидеть список добавленных задач в список задач\n" +
                $"Введя команду /removetask ты сможешь удалять задачи из списка задач\n" +
                $"Введя команду /info ты получишь информацию о версии программы\n" +
                $"Введя команду /exit бот попрощается и завершит работу\n");
            }

        }

        //метод команды Info
        internal static void ShowInfo()
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            Console.WriteLine("Это NailBot версии 1.0 Beta. Релиз {0:dd MMMM yyyy}", releaseDate);
        }


        //метод добавления задачи в List
        internal static void AddTaskList(List<string> tasks)
        {
            Console.WriteLine("Введите описание задачи:");
            string newTask = Console.ReadLine();

            tasks.Add(newTask);

            Console.WriteLine($"Задача \"{newTask}\" добавлена в список задач");

            int taskCounter = 0;
            foreach (var task in tasks)
            {
                taskCounter++;
                Console.WriteLine($"{taskCounter}) {task}");
            }
        }

        //метод добавления задачи в Array
        internal static void AddTaskArray(ref string[] tasks)
        {
            string[] arrayTasks = new string[tasks.Length + 1];

            Console.WriteLine("Введите описание задачи:");
            string newTask = Console.ReadLine();

            int index = arrayTasks.Length - 1;

            arrayTasks[index] = newTask;

            for (int i = 0; i < index; i++)
                arrayTasks[i] = tasks[i];

            tasks = arrayTasks;

            Console.WriteLine($"Задача \"{newTask}\" добавлена в массив задач");
        }

        internal static void ShowArrayTasks(ref string[] tasks)
        {
            try
            {
                if (tasks.Length > 0)
                {
                    Console.WriteLine($"Список задач из массива:\n");
                    for (int i = 0; i < tasks.Length; i++)
                        Console.WriteLine($"{i + 1}) {tasks[i]}");
                    
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Массив пустой");
            }


        }




        //Создайте новую команду /showtasks
        //При вводе команды /showtasks бот должен отобразить список всех добавленных задач.
        //Если задачи ещё не добавлены, необходимо вывести сообщение о том, что список пуст.













        //internal static void AddTaskArray()
        //{
        //    DateTime releaseDate = new DateTime(2025, 02, 08);
        //    Console.WriteLine("Это NailBot версии 1.0 Beta. Релиз {0:dd MMMM yyyy}", releaseDate);
        //}









        internal static bool ExitBot()
        {
            return false;
        }

    }
}
