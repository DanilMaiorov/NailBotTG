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
        ToDoUser toDoUser = new ToDoUser();

        //создаю новый сервис
        ToDoService toDoService = new ToDoService();

        public bool answer = true;

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            //передаю в newService текущий экземпляр update
            toDoService.NewUpdate = update;

            try
            {
                //тут запрашиваю начальные ограничения длины задачи и их количества
                if (update.Message.Id == 1)
                {
                    //передаю в newService созданный экземпляр Chat
                    toDoService.Chat = update.Message.Chat;

                    Init.maxTaskAmount = toDoService.CheckMaxAmount(toDoService.Chat);

                    Init.maxTaskLenght = toDoService.CheckMaxLength(toDoService.Chat);

                    botClient.SendMessage(update.Message.Chat, $"Привет! Это Todo List Bot! Введите команду для начала работы или выхода из бота.\n");

                    Commands.Start.CommandsRender(toDoUser, update);

                    return;
                }

                string input = update.Message.Text;

                Commands command;

                //регулярка на реплейс циферного значения Enum
                input = input.NumberReplacer();

                //верну тут кортежем
                (string inputCommand, string inputText, Guid taskGuid) inputs = toDoService.CheckAddAndRemove(input);


                //реплейс слэша для приведения к Enum 
                input = inputs.Item1.Replace("/", string.Empty);

                if (toDoUser.UserId == Guid.Empty)
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

                        toDoUser = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);

                        Commands.Start.CommandsRender(toDoUser, update);
                        break;

                    case Commands.Help:
                        toDoService.ShowHelp(update.Message.From.Username);
                        break;

                    case Commands.Info:
                        toDoService.ShowInfo();
                        break;

                    case Commands.Addtask:
                        //вызов метода добавления задачи
                        _toDoService.Add(toDoUser, inputs.inputText);
                        break;

                    case Commands.Showtasks:
                        //вызов метода рендера задач
                        toDoService.ShowTasks();
                        break;

                    case Commands.Removetask:
                        //вызов метода удаления задачи
                        _toDoService.Delete(inputs.taskGuid);                         
                        break;

                    case Commands.Exit:
                        Program.Main([input]);
                        break;
                    default:
                        botClient.SendMessage(update.Message.Chat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                        Commands.Start.CommandsRender(toDoUser, update);
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