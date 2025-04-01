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

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {


            try
            {
                if (update.Message.Id == 1)
                {

                    //присваиваю значения длин
                    if (Init.maxTaskAmount == 0)
                        Init.maxTaskAmount = Init.maxTaskAmount.GetStartValues("Введите максимально допустимое количество задач");




                    if (Init.maxTaskLenght == 0)
                        Init.maxTaskLenght = Init.maxTaskLenght.GetStartValues("Введите максимально допустимую длину задачи");

                    botClient.SendMessage(update.Message.Chat, $"Привет! Это Todo List Bot! Введите команду для начала работы или выхода из бота.\n");

                    Commands.Start.CommandsRender(availableEcho, echoNumber);

                    return;
                }






                string res = update.Message.Text;

                Commands command;

                string userName = "";

                string echoText = "";


                //регулярка на реплейс циферного значения Enum
                //input = input.NumberReplacer();


                if (res.StartsWith("/echo "))
                {
                    echoText = res.Substring(6);
                    res = "/echo";
                }

                //реплейс слэша для приведения к Enum
                res = res.Replace("/", string.Empty);

                //приведение к типу Enum
                if (Enum.TryParse<Commands>(res, true, out var result))
                    command = result;
                else
                    command = default;

                switch (command)
                {
                    case Commands.Start:

                        botClient.SendMessage(update.Message.Chat, "Введите ваше имя:");

                        userName = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(userName))
                        {
                            userName = char.ToUpper(userName[0]) + userName.Substring(1);

                            botClient.SendMessage(update.Message.Chat, $"Привет {userName}! Тебе стала доступна команда /echo");

                            botClient.SendMessage(update.Message.Chat, $"Чтобы использовать команду /echo, напиши \"/echo *свой текст*\"\n");

                            availableEcho = true;
                        }
                        else
                            botClient.SendMessage(update.Message.Chat, "Привет, Незнакомец!");

                        Commands.Start.CommandsRender(availableEcho, echoNumber);
                        break;

                    case Commands.Help:
                        Init.ShowHelp(userName, availableEcho);
                        break;

                    case Commands.Info:
                        Init.ShowInfo();
                        break;

                    case Commands.Echo:
                        botClient.SendMessage(update.Message.Chat, "ECHO");
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
                        answer = false;

                        Program.Main([res]);
                        break;
                    default:
                        botClient.SendMessage(update.Message.Chat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                        Commands.Start.CommandsRender(availableEcho, echoNumber);
                        break;
                }
            }
            catch (ArgumentException ex)
            {
                botClient.SendMessage(update.Message.Chat, ex.Message);

                if (update.Message.Id == 1)
                    HandleUpdateAsync(botClient, update);
            }
            catch (TaskCountLimitException ex)
            {
                botClient.SendMessage(update.Message.Chat, ex.Message);

                if (update.Message.Id == 1)
                    HandleUpdateAsync(botClient, update);
            }
            catch (TaskLengthLimitException ex)
            {
                botClient.SendMessage(update.Message.Chat, ex.Message);

                if (update.Message.Id == 1)
                    HandleUpdateAsync(botClient, update);
            }
            catch (DuplicateTaskException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла непредвиденная ошибка");
                throw;
            }
            return;
        }

    }
}



//Создать класс UpdateHandler, который реализует интерфейс IUpdateHandler,
//и перенести в метод HandleUpdateAsync обработку всех команд. Вместо Console.WriteLine использовать SendMessage у ITelegramBotClient

//Перенести try/catch в HandleUpdateAsync. В Main оставить catch(Exception)
//Для вывода в коноль сообщений использовать метод ITelegramBotClient.SendMessage