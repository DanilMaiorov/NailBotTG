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
        //объявляю переменную типа интефрейса IUserService _userService
        private readonly IUserService _userService;

        //объявляю переменную типа интефрейса IToDoService _toDoService
        private readonly IToDoService _toDoService;

        // Получаем IUserService и IToDoService через конструктор
        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _toDoService = toDoService ?? throw new ArgumentNullException(nameof(toDoService));
        }

        enum Commands
        {
            Start = 1, Help, Info, Addtask, Showtasks, Removetask, Exit
        }

        //создаю нового юзера
        ToDoUser newUser = new ToDoUser();

        //создаю новый сервис
        ToDoService newService = new ToDoService();

        public bool answer = true;

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                //тут запрашиваю начальные ограничения длины задачи и их количества
                if (update.Message.Id == 1)
                {
                    Init.maxTaskAmount = newService.CheckMaxAmount(Init.maxTaskAmount, botClient, update);

                    Init.maxTaskLenght = newService.CheckMaxLength(Init.maxTaskLenght, botClient, update);

                    botClient.SendMessage(update.Message.Chat, $"Привет! Это Todo List Bot! Введите команду для начала работы или выхода из бота.\n");

                    Commands.Start.CommandsRender(newUser, botClient, update);

                    return;
                }

                string input = update.Message.Text;

                Commands command;

                //регулярка на реплейс циферного значения Enum
                //input = input.NumberReplacer();

                //верну тут кортежем
                (string, string) inputs = newService.CheckAddAndRemove(input);


                //реплейс слэша для приведения к Enum 
                input = inputs.Item1.Replace("/", string.Empty);

                if (newUser.UserId == Guid.Empty)
                {
                    if (input != "start" && input != "help" && input != "info" && input != "exit")
                        input = "unregistered user command";
                }

                //приведение к типу Enum
                if (Enum.TryParse<Commands>(input, true, out var result))
                    command = result;
                else
                    command = default;

                switch (command)
                {
                    case Commands.Start:

                        newUser = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);

                        Commands.Start.CommandsRender(newUser, botClient, update);
                        break;

                    case Commands.Help:
                        newService.ShowHelp(update.Message.From.Username, botClient, update);
                        break;

                    case Commands.Info:
                        newService.ShowInfo(botClient, update);
                        break;

                    case Commands.Addtask:
                        //вызов метода добавления задачи в List
                        newService.AddTaskList(newUser, inputs.Item2, botClient, update);
                        //вызов метода добавления задачи в Array
                        newService.AddTaskArray(newUser, inputs.Item2, botClient, update);
                        break;

                    case Commands.Showtasks:
                        //вызов метода рендера задач из List
                        newService.ShowTasks(Init.tasksList, botClient, update);
                        //вызов метода рендера задач из Array
                        newService.ShowTasks(ref Init.tasksArray, botClient, update);
                        break;

                    case Commands.Removetask:
                        //вызов метода удаления задачи из List
                        newService.RemoveTaskList(Init.tasksList, inputs.Item2, botClient, update);
                        //вызов метода удаления задачи из Array
                        newService.RemoveTaskArray(ref Init.tasksArray, inputs.Item2, botClient, update);
                        break;

                    case Commands.Exit:
                        Program.Main([input]);
                        break;
                    default:
                        botClient.SendMessage(update.Message.Chat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                        Commands.Start.CommandsRender(newUser, botClient, update);
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
                botClient.SendMessage(update.Message.Chat, ex.Message);
            }
            catch (Exception ex)
            {
                botClient.SendMessage(update.Message.Chat, $"Произошла непредвиденная ошибка");
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