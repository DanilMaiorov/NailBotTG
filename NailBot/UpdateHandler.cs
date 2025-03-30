using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace NailBot
{

    internal class UpdateHandler : IUpdateHandler
    {
        enum Commands
        {
            Start = 1, Help, Info, Echo, Addtask, Showtasks, Removetask, Exit
        }

        static int echoNumber = (int)Commands.Echo;

        //эхо
        static bool availableEcho = false;

        public bool answer = true;

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update) {

            Commands command;

            string res = update.Message.Text;

            string userName = "";

            Chat chatId = update.Message.Chat;

            try
			{
                //приведение к типу Enum
                if (Enum.TryParse<Commands>(update.Message.Text, true, out var result))
                    command = result;
                else
                    command = default;



                switch (command)
                {
                    case Commands.Start:
                        //Console.WriteLine("Введите ваше имя:");

                        botClient.SendMessage(chatId, "Введите ваше имя:");


                        userName = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(userName))
                        {
                            userName = char.ToUpper(userName[0]) + userName.Substring(1);
                            Console.WriteLine($"Привет {userName}! Тебе стала доступна команда /echo");
                            Console.WriteLine($"Чтобы использовать команду /echo, напиши \"/echo *свой текст*\"\n");
                            availableEcho = true;
                        }
                        else
                            Console.WriteLine("Привет, Незнакомец!");
                        Commands.Start.CommandsRender(availableEcho, echoNumber);
                        break;

                    case Commands.Help:
                        Init.ShowHelp(userName, availableEcho);
                        break;

                    case Commands.Info:
                        Init.ShowInfo();
                        break;

                    case Commands.Echo:
                        Console.WriteLine("ECHO");
                        //Console.WriteLine(echoText);
                        break;

                    case Commands.Addtask:
                        //вызов метода добавления задачи в List
                        Init.AddTaskList(Init.tasksList, Init.maxTaskAmount, Init.maxTaskLenght);
                        //вызов метода добавления задачи в Array
                        //Init.AddTaskArray(ref Init.tasksArray, Init.maxTaskAmount, Init.maxTaskLenght);
                        break;

                    case Commands.Showtasks:
                        //вызов метода рендера задач из List
                        Init.ShowTasks(Init.tasksList);
                        //вызов метода рендера задач из Array
                        //Init.ShowTasks(ref Init.tasksArray);
                        break;

                    case Commands.Removetask:
                        //вызов метода удаления задачи из List
                        Init.RemoveTaskList(Init.tasksList);
                        //вызов метода удаления задачи из Array
                        //Init.RemoveTaskArray(ref Init.tasksArray);
                        break;

                    case Commands.Exit:
                        Console.WriteLine("EXIT");
                        answer = false;
                        break;
                    default:
                        Console.WriteLine("Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                        Commands.Start.CommandsRender(availableEcho, echoNumber);
                        break;
                }
            }
			catch (Exception ex)
			{
				throw new Exception("asdsadasd");
			}

        }


    }
}



//Создать класс UpdateHandler, который реализует интерфейс IUpdateHandler,
//и перенести в метод HandleUpdateAsync обработку всех команд. Вместо Console.WriteLine использовать SendMessage у ITelegramBotClient

//Перенести try/catch в HandleUpdateAsync. В Main оставить catch(Exception)
//Для вывода в коноль сообщений использовать метод ITelegramBotClient.SendMessage