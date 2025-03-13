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

        //работа с List
        //метод добавления задачи в List
        internal static void AddTaskList(List<string> tasks)
        {
            Console.WriteLine("Введите описание задачи (добавится в List):");
            string newTask = Console.ReadLine();

            tasks.Add(newTask);

            Console.WriteLine($"Задача \"{newTask}\" добавлена в List задач");
        }

        //добавлю перегрузку
        internal static void AddTaskList(List<string> tasks, int maxTasksAmount, int taskLengthLimit)
        {
            //проверяю длину листа и выбрасываю исключение если больше лимита
            if (tasks.Count >= maxTasksAmount)
                throw new TaskCountLimitException($"Превышено максимальное количество задач равное {maxTasksAmount}", maxTasksAmount);

            Console.WriteLine("Введите описание задачи (добавится в List):");
            string newTask = Console.ReadLine();

            //проверяю длину введёной задачи и выбрасываю исключение если больше лимита
            if (newTask.Length >= taskLengthLimit)
                throw new TaskLengthLimitException($"Длина задачи \"{newTask}\" - {newTask.Length}, что превышает максимально допустимое значение {taskLengthLimit}", newTask.Length, taskLengthLimit);

            tasks.Add(newTask);

            Console.WriteLine($"Задача \"{newTask}\" добавлена в List задач");
        }



        //метод рендера задач из List
        internal static void ShowTasks(List<string> tasks)
        {
            if (tasks.Count == 0)
                Console.WriteLine("Список задач из List пуст");
            else
            {
                Console.WriteLine($"Список задач из List:\n");
                int taskCounter = 0;
                foreach (string task in tasks)
                {
                    taskCounter++;
                    Console.WriteLine($"{taskCounter}) {task}");
                }
            }
        }
        
        //метод удаления задачи из List
        internal static void RemoveTaskList(List<string> tasks)
        {
            if (tasks.Count == 0)
            {
                Console.WriteLine("Ваш список задач пуст, удалять нечего:");
                return;
            }
            ShowTasks(tasks);

            Console.Write("Введите номер задачи для удаления (из List): ");
            int taskNumber;

            bool success = int.TryParse(Console.ReadLine(), out taskNumber);

            if (success && (taskNumber > 0 && taskNumber <= tasks.Count))
            {
                Console.WriteLine($"Задача {tasks[taskNumber - 1]} удалена");
                tasks.RemoveAt(taskNumber - 1);
                ShowTasks(tasks);
            }
            else
            {
                Console.WriteLine("Введён некорректный номер задачи");
                RemoveTaskList(tasks);
            }
        }

        //работа с Array

        //метод добавления задачи в Array
        internal static void AddTaskArray(ref string[] tasks, int maxTasksAmount, int taskLengthLimit)
        {
            string[] arrayTasks = new string[tasks.Length + 1];


            //проверяю длину листа и выбрасываю исключение если больше лимита
            if (tasks.Length >= maxTasksAmount)
                throw new TaskCountLimitException($"Превышено максимальное количество задач равное {maxTasksAmount}", maxTasksAmount);


            Console.WriteLine("Введите описание задачи (добавится в Array):");
            string newTask = Console.ReadLine();

            //проверяю длину введёной задачи и выбрасываю исключение если больше лимита
            if (newTask.Length >= taskLengthLimit)
                throw new TaskLengthLimitException($"Длина задачи \"{newTask}\" - {newTask.Length}, что превышает максимально допустимое значение {taskLengthLimit}", newTask.Length, taskLengthLimit);

            int index = arrayTasks.Length - 1;

            arrayTasks[index] = newTask;

            for (int i = 0; i < index; i++)
                arrayTasks[i] = tasks[i];

            tasks = arrayTasks;

            Console.WriteLine($"Задача \"{newTask}\" добавлена в Array задач");
        }

        //метод рендера задач из Array
        internal static void ShowTasks(ref string[] tasks)
        {
            if (tasks.Length == 0)
                Console.WriteLine("Список задач из Array пуст");
            else
            {
                Console.WriteLine($"Список задач из Array:\n");
                for (int i = 0; i < tasks.Length; i++)
                    Console.WriteLine($"{i + 1}) {tasks[i]}");
            }
        }

        //метод удаления задачи из Array
        internal static void RemoveTaskArray(ref string[] tasks)
        {
            if (tasks.Length == 0)
            {
                Console.WriteLine("Ваш список задач пуст, удалять нечего:");
                return;
            }

            ShowTasks(ref tasks);

            Console.Write("Введите номер задачи для удаления (из Array): ");
            int taskNumber;

            bool success = int.TryParse(Console.ReadLine(), out taskNumber);

            if (success && (taskNumber > 0 && taskNumber <= tasks.Length))
            {
                Console.WriteLine($"Задача {tasks[taskNumber - 1]} удалена");

                string []newTasks = new string[tasks.Length - 1];

                int index = taskNumber;

                for (int i = 0; i < index - 1; i++)
                    newTasks[i] = tasks[i];

                for (int i = index - 1; i < newTasks.Length; i++)
                    newTasks[i] = tasks[i + 1];

                tasks = newTasks;

                ShowTasks(ref tasks);
            }
            else
            {
                Console.WriteLine("Введён некорректный номер задачи");
                RemoveTaskArray(ref tasks);
            }
        }
    }
}
