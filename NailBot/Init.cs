using System.Xml.Linq;

namespace NailBot
{
    internal class Init
    {
        public static void Start()
        {
            string[] availableCommands = { "/start", "/help", "/info", "/exit" };

            Console.WriteLine($"Привет! Это NailBot! \nСписок доступных команд:");

            Commands(ref availableCommands);

            Console.WriteLine("Введите команду для начала работы или выхода из бота");

            string startInput = Console.ReadLine();

            string userName = "Пользователь";

            bool echoAvailable = false;

            while (Handler(startInput, ref userName, ref availableCommands, ref echoAvailable))
            {
                startInput = Console.ReadLine();
            }
            userName = echoAvailable ? userName : "Незнакомец";
            Console.WriteLine($"До свидания, {userName}! До новых встреч!");
            Console.ReadLine();
        }


        public static bool Handler(string input, ref string name, ref string[] array, ref bool echoAvailable)
        {           
            string userName = name;

            bool answer = true;

            switch (input)
            {
                case "/start":
                    Console.WriteLine("Введите своё имя");
                    name = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(name) && name != userName)
                    {
                        Console.WriteLine($"Привет {name}! Тебе стала доступна команда /echo");
                        Console.WriteLine($"Чтобы использовать команду /echo, напиши \"/echo *свой текст*\"");
                        echoAvailable = true;
                    } else
                    {
                        Console.WriteLine("Привет, Незнакомец!");
                    }
                    answer = true;
                    break;
                case string echo when Extension.Slicer(input) == "/echo" && echoAvailable:
                    Console.WriteLine(input.Substring(5).Trim());
                    break;
                case "/help":
                    Console.WriteLine("Это NailBot - телеграм бот для записи на наготочки. Я умею запоминать имя и записывать на ноготочки на текущий и следующий месяцы");
                    answer = true;
                    break;
                case "/info":
                    DateTime releaseDate = new DateTime(2024, 11, 25);
                    Console.WriteLine("Это NailBot версии 1.0 Beta. Релиз {0:dd MMMM yyyy}", releaseDate);
                    answer = true;
                    break;
                case "/exit":
                    answer = false;
                    break;
                default:
                    Console.WriteLine("\nВы ввели некорректную команду. Вот список доступных команд:");
                    if (echoAvailable)
                    {
                        Commands("/echo", ref array);
                    } else
                    {
                        Commands(ref array);
                    }
                    answer = true;
                    break;
            }
            return answer;
        }
        
        public static void Commands(string item, ref string[] array)
        {
            if (array.Length < 5)
            {
                string[] updatedAvailableCommands = new string[array.Length + 1];

                for (int i = 0; i < array.Length; i++)
                {
                    updatedAvailableCommands[i] = array[i];
                }
                updatedAvailableCommands[updatedAvailableCommands.Length - 1] = item;
                array = updatedAvailableCommands;

                for (int i = 0; i < array.Length; i++)
                {
                    Console.WriteLine($"{i + 1}) {array[i]}");
                }
            } 
            else
            {
                for (int i = 0; i < array.Length; i++)
                {
                    Console.WriteLine($"{i + 1}) {array[i]}");
                }
            }

        }
        public static void Commands(ref string[] array)
        {
            string[] availableCommands = { "/start", "/help", "/info", "/exit" };
            for (int i = 0; i < availableCommands.Length; i++)
            {
                Console.WriteLine($"{i + 1}) {availableCommands[i]}");
            }
        }
    }
}
