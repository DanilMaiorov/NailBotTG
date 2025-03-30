using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using static NailBot.Extensions;
using static NailBot.StartValues;

namespace NailBot
{
    internal class Init
    {
        enum Commands
        {
            Start = 1, Help, Info, Echo, Addtask, Showtasks, Removetask, Exit
        }

        static int echoNumber = (int)Commands.Echo;

        //количество задач при запуске программы
        public static int maxTaskAmount = 0;

        //длина задачи при запуске программы
        public static int maxTaskLenght = 0;

        //эхо
        static bool availableEcho = false;

        //дефолтное имя пользака
        static string userName = "Пользователь";

        //объявление списка задач в виде List
        static List<string> tasksList = new List<string>();
        
        //объявление списка задач в виде Array
        static string[] tasksArray = [];



        //МЕТОДЫ КОМАНД
        //метод команды Help
        internal static void ShowHelp(string userName, bool availableEcho)
        {
            if (availableEcho)
            {
                Console.WriteLine($"{userName}, это Todo List Bot - телеграм бот записи дел.\n" +
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
                Console.WriteLine($"{userName}, Todo List Bot - телеграм бот записи дел.\n" +
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
            string newTask = Validate.ValidateString(Console.ReadLine());

            tasks.Add(newTask);

            Console.WriteLine($"Задача \"{newTask}\" добавлена в List задач");
        }

        //добавлю перегрузку метода добавления задачи в List
        internal static void AddTaskList(List<string> tasks, int maxTasksAmount, int taskLengthLimit)
        {
            //проверяю длину листа и выбрасываю исключение если больше лимита
            if (tasks.Count >= maxTasksAmount)
                throw new TaskCountLimitException(maxTasksAmount);

            Console.WriteLine("Введите описание задачи (добавится в List):");

            //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
            string newTask = Validate.ValidateString(Console.ReadLine(), taskLengthLimit);

            //проверяю дубликаты введённой задачи
            if (tasks.Contains(newTask))
                throw new DuplicateTaskException(newTask);
            //throw new DuplicateTaskException($"Задача \"{newTask}\" является дубликатом, она не будет добавлена", newTask);

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
        internal static void AddTaskArray(ref string[] tasks)
        {
            string[] arrayTasks = new string[tasks.Length + 1];

            Console.WriteLine("Введите описание задачи (добавится в Array):");
            //валидация строки
            string newTask = Validate.ValidateString(Console.ReadLine());

            int index = arrayTasks.Length - 1;

            arrayTasks[index] = newTask;

            for (int i = 0; i < index; i++)
                arrayTasks[i] = tasks[i];

            tasks = arrayTasks;

            Console.WriteLine($"Задача \"{newTask}\" добавлена в Array задач");
        }

        //добавлю перегрузку метода добавления задачи в Array
        internal static void AddTaskArray(ref string[] tasks, int maxTasksAmount, int taskLengthLimit)
        {
            string[] arrayTasks = new string[tasks.Length + 1];

            //проверяю длину  и выбрасываю исключение если больше лимита
            if (tasks.Length >= maxTasksAmount)
                throw new TaskCountLimitException(maxTasksAmount);


            Console.WriteLine("Введите описание задачи (добавится в Array):");

            //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
            string newTask = Validate.ValidateString(Console.ReadLine(), taskLengthLimit);


            //проверяю дубликаты введённой задачи
            for (int i = 0; i < tasks.Length; i++)
            {
                if (tasks[i] == newTask)
                    throw new DuplicateTaskException(newTask);
                //throw new DuplicateTaskException($"Задача \"{newTask}\" является дубликатом, она не будет добавлена", newTask);
            }

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

                string[] newTasks = new string[tasks.Length - 1];

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

        public static void Start(int tasksAmount, int taskLenght)
        {
            //присваиваю значения длин
            if (tasksAmount == 0)
                maxTaskAmount = tasksAmount.GetStartValues("Введите максимально допустимое количество задач");

            if (maxTaskLenght == 0)
                maxTaskLenght = taskLenght.GetStartValues("Введите максимально допустимую длину задачи");
            
            //создаю экземпляр класса со значениями длин для хранения
            StartValues StartValues = new StartValues(maxTaskAmount, maxTaskLenght);

            //рендер списка команд
            Commands.Start.CommandsRender(availableEcho, echoNumber);

            string startInput = Validate.ValidateString(Console.ReadLine());

            while (Handle(startInput))
            {
                startInput = Validate.ValidateString(Console.ReadLine());
            }

            userName = availableEcho ? userName : "Незнакомец";
            Console.WriteLine($"До свидания, {userName}! До новых встреч!");
            Console.ReadKey();
        }

        public static bool Handle(string input)
        {
            string echoText = "";
            bool answer = true;

            if (input.StartsWith("/echo "))
            {
                echoText = input.Substring(6);
                input = "/echo";
            }

            //реплейс слэша для приведения к Enum
            input = input.Replace("/", string.Empty);

            Commands command;

            //регулярка на реплейс циферного значения Enum
            input = input.NumberReplacer();

            //приведение к типу Enum
            if (Enum.TryParse<Commands>(input, true, out var result))
                command = result;
            else
                command = default;

            switch (command)
            {
                case Commands.Start:
                    Console.WriteLine("Введите ваше имя:");
                    userName = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(userName))
                    {
                        userName = char.ToUpper(userName[0]) + userName.Substring(1);
                        Console.WriteLine($"Привет {userName}! Тебе стала доступна команда /echo");
                        Console.WriteLine($"Чтобы использовать команду /echo, напиши \"/echo *свой текст*\"\n");
                        availableEcho = true;
                        Commands.Start.CommandsRender(availableEcho, echoNumber);
                    }
                    else
                        Console.WriteLine("Привет, Незнакомец!");
                    break;

                case Commands.Help:
                    ShowHelp(userName, availableEcho);
                    break;

                case Commands.Info:
                    ShowInfo();
                    break;

                case Commands.Echo:
                    Console.WriteLine(echoText);
                    break;

                case Commands.Addtask:
                    //вызов метода добавления задачи в List
                    AddTaskList(tasksList, maxTaskAmount, maxTaskLenght);
                    //вызов метода добавления задачи в Array
                    AddTaskArray(ref tasksArray, maxTaskAmount, maxTaskLenght);
                    break;

                case Commands.Showtasks:
                    //вызов метода рендера задач из List
                    ShowTasks(tasksList);
                    //вызов метода рендера задач из Array
                    ShowTasks(ref tasksArray);
                    break;

                case Commands.Removetask:
                    //вызов метода удаления задачи из List
                    RemoveTaskList(tasksList);
                    //вызов метода удаления задачи из Array
                    RemoveTaskArray(ref tasksArray);
                    break;

                case Commands.Exit:
                    answer = false;
                    break;
                default:
                    Console.WriteLine("Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                    Commands.Start.CommandsRender(availableEcho, echoNumber);
                    break;
            }
            return answer;
        }
    }
}