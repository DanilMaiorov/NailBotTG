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


        public static void Start(int tasksAmount, int taskLenght)
        {
            if (tasksAmount == 0)
            {
                //спрашиваем при запуске программы до тех пор пока не получим валидное значение
                Console.WriteLine("Введите максимально допустимое количество задач");

                string strTaskCountLimit = Console.ReadLine();

                //переопределение количества задач при успешном парсинге
                maxTaskAmount = Validate.ParseAndValidateInt(strTaskCountLimit, 1, 100);
            }

            if (taskLenght == 0)
            {
                //спрашиваем при запуске программы до тех пор пока не получим валидное значение
                Console.WriteLine("Введите максимально допустимую длину задачи");

                string strTaskLengthLimit = Console.ReadLine();

                //переопределение количества задач при успешном парсинге
                maxTaskLenght = Validate.ParseAndValidateInt(strTaskLengthLimit, 1, 100);
            }

            Console.WriteLine($"Привет! Это NailBot!" + 
            " Введите команду для начала работы или выхода из бота.\n");

            Extensions.CommandsRender(Commands.Start, availableEcho, echoNumber);

            string startInput = Console.ReadLine();


            while (Handler(startInput, ref userName, ref availableEcho, tasksList, ref tasksArray, maxTaskAmount, maxTaskLenght))
            {
                startInput = Console.ReadLine();
            }

            userName = availableEcho ? userName : "Незнакомец";
            Console.WriteLine($"До свидания, {userName}! До новых встреч!");
            Console.ReadKey();
        }

        public static bool Handler(string input, ref string name, ref bool echo, List<string> listTasks, ref string[] arrayTasks, int taskCountLimit, int taskLengthLimit)
        {
            string userName = name;
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
            //input = Extensions.NumberReplacer(input);

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
                        Extensions.CommandsRender(Commands.Start, echo, echoNumber);
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
                    CommandsList.AddTaskList(listTasks, taskCountLimit, taskLengthLimit);
                    //вызов метода добавления задачи в Array
                    //CommandsList.AddTaskArray(ref arrayTasks, taskCountLimit, taskLengthLimit);
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
//вынес создание листа и массива из метода start
//вынес объявление имени пользака и эха из метода старт