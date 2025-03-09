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

        public static void Start()
        {
            string userName = "Пользователь";

            bool availableEcho = false;

            Console.WriteLine($"Привет! Это NailBot!" + 
            " Введите команду для начала работы или выхода из бота.\n");

            Extensions.CommandsRender(Commands.Start, availableEcho, echoNumber);

            string startInput = Console.ReadLine();


            //объявление списка задач в виде списка
            List<string> tasksList = new List<string>();

            //объявление списка задач в виде списка
            string[] tasksArray = [];


            while (Handler(startInput, ref userName, ref availableEcho, tasksList, ref tasksArray))
            {
                startInput = Console.ReadLine();
            }

            userName = availableEcho ? userName : "Незнакомец";
            Console.WriteLine($"До свидания, {userName}! До новых встреч!");
            Console.ReadKey();
        }


        public static bool Handler(string input, ref string name, ref bool echo, List<string> listTasks, ref string[] arrayTasks)
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
                    //CommandsList.AddTaskList(listTasks);

                    CommandsList.AddTaskArray(ref arrayTasks);
                    break;

                case Commands.Showtasks:
                    //CommandsList.ShowArrayTasks(listTasks);

                    CommandsList.ShowArrayTasks(ref arrayTasks);
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