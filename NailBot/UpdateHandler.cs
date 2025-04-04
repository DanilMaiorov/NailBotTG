using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static NailBot.IUserService;


namespace NailBot
{

    internal class UpdateHandler : IUpdateHandler
    {

        private readonly IUserService _userService;

        // Получаем IUserService через конструктор
        public UpdateHandler(IUserService userService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        enum Commands
        {
            Start = 1, Help, Info, Addtask, Showtasks, Removetask, Exit
        }

        User newUser = new User();

        public ITelegramBotClient botClient1;
        public Update update1 = new Update();

        public void SayGoodBye()
        {
            botClient1.SendMessage(update1.Message.Chat, $"ПАКА");
        }

        public bool answer = true;

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {

            update1 = update;
            botClient1 = botClient;

            
            try
            {
                if (update.Message.Id == 1)
                {
                    //присваиваю значения длин
                    if (Init.maxTaskAmount == 0)
                        Init.maxTaskAmount = Init.maxTaskAmount.GetStartValues("Введите максимально допустимое количество задач", botClient, update);

                    if (Init.maxTaskLenght == 0)
                        Init.maxTaskLenght = Init.maxTaskLenght.GetStartValues("Введите максимально допустимую длину задачи", botClient, update);

                    botClient.SendMessage(update.Message.Chat, $"Привет! Это Todo List Bot! Введите команду для начала работы или выхода из бота.\n");


                    Console.WriteLine(newUser.UserId == Guid.Empty);

                    //_userService.GetUser(update.Message.From.Id);


                    Commands.Start.CommandsRender(newUser);

                    return;

                }

                string res = update.Message.Text;

                Commands command;

                string userName = "";


                //регулярка на реплейс циферного значения Enum
                //input = input.NumberReplacer();

                //реплейс слэша для приведения к Enum 
                res = res.Replace("/", string.Empty);

                if (newUser.UserId == Guid.Empty)
                {
                    if (res != "start" && res != "help" && res != "info" && res != "exit")
                        res = "unregistered user command";
                }




                //приведение к типу Enum
                if (Enum.TryParse<Commands>(res, true, out var result))
                    command = result;
                else
                    command = default;

                switch (command)
                {
                    case Commands.Start:

                        newUser = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);

                        Commands.Start.CommandsRender(newUser);
                        break;

                    case Commands.Help:
                        Init.ShowHelp();
                        break;

                    case Commands.Info:
                        Init.ShowInfo();
                        break;

                    case Commands.Addtask:
                        //вызов метода добавления задачи в List
                        Init.AddTaskList(Init.tasksList, Init.maxTaskAmount, Init.maxTaskLenght);
                        //вызов метода добавления задачи в Array
                        Init.AddTaskArray(ref Init.tasksArray, Init.maxTaskAmount, Init.maxTaskLenght);
                        break;

                    case Commands.Showtasks:
                        //вызов метода рендера задач из List
                        Init.ShowTasks(Init.tasksList);
                        //вызов метода рендера задач из Array
                        Init.ShowTasks(ref Init.tasksArray);
                        break;

                    case Commands.Removetask:
                        //вызов метода удаления задачи из List
                        Init.RemoveTaskList(Init.tasksList);
                        //вызов метода удаления задачи из Array
                        Init.RemoveTaskArray(ref Init.tasksArray);
                        break;

                    case Commands.Exit:
                        Program.Main([res]);
                        break;
                    default:
                        botClient.SendMessage(update.Message.Chat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                        Commands.Start.CommandsRender(newUser);
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