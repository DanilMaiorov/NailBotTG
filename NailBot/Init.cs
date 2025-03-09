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
            Start = 1, Help, Info, Echo, Exit
        }

        static int echoNumber = (int)Commands.Echo;

        public static void Start()
        {
            string userName = "Пользователь";

            bool availableEcho = false;

            Console.WriteLine($"Привет! Это NailBot!");

            Console.WriteLine("Введите команду для начала работы или выхода из бота.");

            Extensions.CommandsRender(Commands.Start, availableEcho, echoNumber);

            string startInput = Console.ReadLine();

            while (Handler(startInput, ref userName, ref availableEcho))
            {
                startInput = Console.ReadLine();
            }

            userName = availableEcho ? userName : "Незнакомец";
            Console.WriteLine($"До свидания, {userName}! До новых встреч!");
            Console.ReadKey();
        }


        public static bool Handler(string input, ref string name, ref bool echo)
        {
            string userName = name;
            string echoText = "";
            bool answer = true;

            if (input.StartsWith("/echo "))
            {
                //StringBuilder sb = new StringBuilder(input);
                //echoText = sb.ToString();

                //StringBuilder sb = new StringBuilder(input);
                echoText = input.Substring(6);

                input = "/echo";
            }

            input = input.Replace("/", string.Empty);

            //try
            //{
                Commands command;

                input = Extensions.NumberReplacer(input);

                if (Enum.TryParse<Commands>(input, true, out var result))
                {
                    int commandNumber = (int)result;
                    Console.WriteLine("Номер поля " + commandNumber);
                    command = result;
                }
                else
                {
                    Console.WriteLine("Нет соответствующего значения");
                    command = default;
                }



                //Commands command = (Commands)Enum.Parse(typeof(Commands), input.Replace("/", string.Empty), true);

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
                        Console.WriteLine($"{name}, это NailBot - телеграм бот для записи на ноготочки.\n" +
                        $"Введя команду /start бот предложит тебе ввести имя\n" +
                        $"Введя команду /help ты получишь справку о командах\n" +
                        $"Введя команду /echo и какой либо текст после - этот текст останется на консоли\n" +
                        $"Введя команду /info ты получишь информацию о версии программы\n" +
                        $"Введя команду /exit бот попрощается и завершит работу\n");
                        break;

                    case Commands.Info:
                        DateTime releaseDate = new DateTime(2025, 02, 08);
                        Console.WriteLine("Это NailBot версии 1.0 Beta. Релиз {0:dd MMMM yyyy}", releaseDate);
                        break;
                    case Commands.Echo:
                        Console.WriteLine(echoText);
                        break;
                    case Commands.Exit:
                        answer = false;
                        break;
                    default:
                        Console.WriteLine("Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                        Extensions.CommandsRender(Commands.Start, echo, echoNumber);
                        break;
                }
            //}
            //catch (ArgumentException)
            //{
            //    Console.WriteLine("Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
            //    Extensions.CommandsRender(Commands.Start, echo, echoNumber);
            //}

            return answer;
        }
    }
}
