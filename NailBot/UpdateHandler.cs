using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.ComponentModel.DataAnnotations;

namespace NailBot
{
    public enum Commands
    {
        Start = 1, Help, Info, Addtask, Showtasks, Showalltasks, Removetask, Completetask, Exit
    }

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

        //создаю новый туДуСервис
        ToDoService toDoService = new ToDoService();

        //создаю новый юзерСервис
        //UserService userService = new UserService();

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                //получаю текущего юзера
                var currentUser = _userService.GetUser(update.Message.From.Id);

                //присвою так
                _toDoService.MaxTaskAmount = toDoService.MaxTaskAmount;
                _toDoService.MaxTaskLenght = toDoService.MaxTaskLenght;
                _toDoService.TasksList = toDoService.TasksList;



                //тут запрашиваю начальные ограничения длины задачи и их количества
                if (update.Message.Id == 1)
                {
                    //передаю в newService созданный экземпляр Chat
                    toDoService.Chat = update.Message.Chat;

                    toDoService.MaxTaskAmount = toDoService.CheckMaxAmount(toDoService.Chat);

                    toDoService.MaxTaskLenght = toDoService.CheckMaxLength(toDoService.Chat);

                    Init.botClient.SendMessage(update.Message.Chat, $"Привет! Это Todo List Bot! Введите команду для начала работы или выхода из бота.\n");

                    Commands.Start.CommandsRender(currentUser, update);

                    return;
                }

                string input = update.Message.Text;

                Commands command;

                //регулярка на реплейс циферного значения Enum
                input = input.NumberReplacer();

                //верну тут кортежем
                (string inputCommand, string inputText, Guid taskGuid) inputs = toDoService.InputCheck(input);

                //реплейс слэша для приведения к Enum 
                input = inputs.inputCommand.Replace("/", string.Empty);

                if (currentUser == null)
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
                        if (currentUser == null)
                            //регаю нового юзера
                            currentUser = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);

                        //рендерю список команд
                        Commands.Start.CommandsRender(currentUser, update);
                        break;

                    case Commands.Help:
                        toDoService.ShowHelp(currentUser);
                        break;

                    case Commands.Info:
                        toDoService.ShowInfo();
                        break;

                    case Commands.Addtask:
                        //вызов метода добавления задачи
                        _toDoService.Add(currentUser, inputs.inputText);
                        break;

                    case Commands.Showtasks:
                        //вызов метода рендера задач
                        toDoService.ShowTasks();
                        break;

                    case Commands.Showalltasks:
                        //вызов метода рендера задач
                        toDoService.ShowAllTasks();
                        break;

                    case Commands.Removetask:
                        //вызов метода удаления задачи
                        _toDoService.Delete(inputs.taskGuid);
                        break;

                    case Commands.Completetask:
                        //вызов метода удаления задачи
                        _toDoService.MarkCompleted(inputs.taskGuid);
                        break;

                    case Commands.Exit:
                        Console.WriteLine("Exit");
                        break;
                    default:
                        Init.botClient.SendMessage(update.Message.Chat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                        Commands.Start.CommandsRender(currentUser, update);
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