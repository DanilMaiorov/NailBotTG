using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

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
        public static int startTaskAmount = 0;

        //объявление списка задач в виде List
        static List<string> tasksList = new List<string>();

        public static void Start(int tasksAmount)
        {
            int taskCountLimit = tasksAmount;
            if (tasksAmount == 0)
            {
                //спрашиваем при запуске программы до тех пор пока не получим валидное значение
                Console.WriteLine("Введите максимально допустимое количество задач");

                taskCountLimit = int.Parse(Console.ReadLine());

                if (taskCountLimit > 0 && taskCountLimit > 100)
                    throw new ArgumentException();

                //переопределение количества задач при успешном парсинге
                startTaskAmount = taskCountLimit;
            }










        string userName = "Пользователь";

            bool availableEcho = false;

            Console.WriteLine($"Привет! Это NailBot!" + 
            " Введите команду для начала работы или выхода из бота.\n");

            Extensions.CommandsRender(Commands.Start, availableEcho, echoNumber);

            string startInput = Console.ReadLine();



            //объявление списка задач в виде Array
            string[] tasksArray = [];

            while (Handler(startInput, ref userName, ref availableEcho, tasksList, ref tasksArray, taskCountLimit))
            {
                startInput = Console.ReadLine();
            }

            userName = availableEcho ? userName : "Незнакомец";
            Console.WriteLine($"До свидания, {userName}! До новых встреч!");
            Console.ReadKey();
        }

        public static bool Handler(string input, ref string name, ref bool echo, List<string> listTasks, ref string[] arrayTasks, int taskCountLimit)
        {
            string userName = name;
            string echoText = "";
            bool answer = true;

            if (input.StartsWith("/echo "))
            {
                echoText = input.Substring(6);
                input = "/echo";
            }


            Console.WriteLine($"Максимальное количество задач {taskCountLimit}");













            //реплейс слэша для приведения к Enum
            input = input.Replace("/", string.Empty);

            Commands command;

            //регулярка на реплейс циферного значения Enum
            input = Extensions.NumberReplacer(input);

            //приведение к типу Enum
            if (Enum.TryParse<Commands>(input, true, out var result))
                command = result;
            else
                command = default;

            switch (command)
            {
                case Commands.Start:
                    Console.WriteLine("Введите ваше имя:");
                    name = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(name) && name != userName)
                    {
                        name = char.ToUpper(name[0]) + name.Substring(1);
                        Console.WriteLine($"Привет {name}! Тебе стала доступна команда /echo");
                        Console.WriteLine($"Чтобы использовать команду /echo, напиши \"/echo *свой текст*\"");
                        echo = true;
                    }
                    else
                        Console.WriteLine("Привет, Незнакомец!");
                    break;

                case Commands.Help:
                    CommandsList.ShowHelp(name, echo);
                    break;

                case Commands.Info:
                    CommandsList.ShowInfo();
                    break;

                case Commands.Echo:
                    Console.WriteLine(echoText);
                    break;

                case Commands.Addtask:
                    //вызов метода добавления задачи в List
                    CommandsList.AddTaskList(listTasks, taskCountLimit);
                    //вызов метода добавления задачи в Array
                    //CommandsList.AddTaskArray(ref arrayTasks);
                    break;

                case Commands.Showtasks:
                    //вызов метода рендера задач из List
                    CommandsList.ShowTasks(listTasks);
                    //вызов метода рендера задач из Array
                    //CommandsList.ShowTasks(ref arrayTasks);
                    break;

                case Commands.Removetask:
                    //вызов метода удаления задачи из List
                    CommandsList.RemoveTaskList(listTasks);
                    //вызов метода удаления задачи из Array
                    //CommandsList.RemoveTaskArray(ref arrayTasks);
                    break;

                case Commands.Exit:
                    answer = false;
                    break;
                default:
                    Console.WriteLine("Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                    Extensions.CommandsRender(Commands.Start, echo, echoNumber);
                    break;
            }
            return answer;
        }
    }
}

//решил сделать циклом while
//добавил стартовое количество задач и добавил в стартовый метод параметр стартового количества задач
//вынес создание листа из метода