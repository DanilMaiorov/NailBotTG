using NailBot.Core.Entities;
using NailBot.Core.Exceptions;
using NailBot.Core.Services;
using NailBot.Helpers;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace NailBot.TelegramBot;
public enum Commands
{
    Start = 1, Help, Info, Addtask, Showtasks, Showalltasks, Removetask, Find, Completetask, Report, Exit
}

internal class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;

    private readonly IToDoReportService _toDoReportService;

    private readonly ToDoService toDoService;
    private readonly UserService userService;

    public UpdateHandler(IUserService iuserService, IToDoService itoDoService, IToDoReportService itoDoReportService)
    {
        _userService = iuserService ?? throw new ArgumentNullException(nameof(iuserService));
        _toDoService = itoDoService ?? throw new ArgumentNullException(nameof(itoDoService));

        _toDoReportService = itoDoReportService ?? throw new ArgumentNullException(nameof(itoDoReportService));

        //явно приведу к типу
        toDoService = (ToDoService)itoDoService;
        userService = (UserService)iuserService;
    }

    public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
    {
        //получаю экземпляр чата
        var currentChat = update.Message.Chat;
        Console.WriteLine();
        try
        {            
            //получаю текущего юзера
            var currentUser = _userService.GetUser(update.Message.From.Id);

            //получаю весь список задач текущего юзера если user не null
            var currentUserTaskList = currentUser != null
                ? _toDoService.GetAllByUserId(currentUser.UserId) 
                : null;
            
            //тут запрашиваю начальные ограничения длины задачи и их количества
            if (update.Message.Id == 1)
            {
                botClient.SendMessage(currentChat, $"Привет! Это Todo List Bot! Введите команду для начала работы или выхода из бота.\n");

                Commands.Start.CommandsRender(currentUser, currentChat, botClient);

                return;
            }

            string input = update.Message.Text;

            Commands command;

            //регулярка на реплейс циферного значения Enum
            input = input.NumberReplacer();

            //верну тут кортежем
            (string inputCommand, string inputText, Guid taskGuid) inputs = Helper.InputCheck(input, currentUserTaskList);

            //реплейс слэша для приведения к Enum 
            input = inputs.inputCommand.Replace("/", string.Empty);

            if (currentUser == null)
            {
                if (input != "start" && input != "help" && input != "info" && input != "exit")
                {
                    input = "unregistered user command";
                }
            }

            //приведение к типу Enum
            if (Enum.TryParse<Commands>(input, true, out var result))
            {
                command = result;
            }
            else
            {
                command = default;
            }
                
            switch (command)
            {
                case Commands.Start:
                    if (currentUser == null)
                        //регаю нового юзера
                        currentUser = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);

                    //рендерю список команд
                    Commands.Start.CommandsRender(currentUser, currentChat, botClient);
                    break;

                case Commands.Help:
                    ShowHelp(currentUser);
                    break;

                case Commands.Info:
                    ShowInfo();
                    break;

                case Commands.Addtask:
                    //вызов метода добавления задачи
                    var newTask = _toDoService.Add(currentUser, inputs.inputText);

                    botClient.SendMessage(currentChat, $"Задача \"{newTask.Name}\" добавлена в список задач.\n");
                    break;

                case Commands.Showtasks:
                    //вызов метода рендера задач
                    ShowTasks(currentUser.UserId);
                    break;

                case Commands.Showalltasks:
                    //вызов метода рендера задач
                    ShowTasks(currentUser.UserId, true);
                    break;

                case Commands.Removetask:
                    //вызов метода удаления задачи
                    _toDoService.Delete(inputs.taskGuid);

                    botClient.SendMessage(currentChat, $"Задача {inputs.taskGuid} удалена.\n");
                    break;

                case Commands.Completetask:
                    //вызов метода выполнения задачи
                    _toDoService.MarkCompleted(inputs.taskGuid);

                    botClient.SendMessage(currentChat, $"Задача {inputs.taskGuid} выполнена.\n");
                    break;

                case Commands.Find:
                    //вызов метода поиска задачи
                    var findedTasks = _toDoService.Find(currentUser, inputs.inputText);
                    ShowTasks(currentUser.UserId, false, findedTasks);
                    break;

                case Commands.Report:
                    //вызов метода печати отчета
                    var stats = _toDoReportService.GetUserStats(currentUser.UserId);
                    botClient.SendMessage(currentChat, $"Статистика по задачам на {stats.generatedAt}. Всего: {stats.total}; Завершенных: {stats.completed}; Активных: {stats.active};");
                    break;

                case Commands.Exit:
                    Console.WriteLine("Exit");
                    break;
                
                default:
                    botClient.SendMessage(currentChat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                    Commands.Start.CommandsRender(currentUser, currentChat, botClient);
                    break;
            }
        }
        catch (ArgumentException ex)
        {
            botClient.SendMessage(currentChat, ex.Message);

            if (update.Message.Id == 1)
                HandleUpdateAsync(botClient, update);
        }
        catch (TaskCountLimitException ex)
        {
            botClient.SendMessage(currentChat, ex.Message);

            if (update.Message.Id == 1)
                HandleUpdateAsync(botClient, update);
        }
        catch (TaskLengthLimitException ex)
        {
            botClient.SendMessage(currentChat, ex.Message);

            if (update.Message.Id == 1)
                HandleUpdateAsync(botClient, update);
        }
        catch (DuplicateTaskException ex)
        {
            botClient.SendMessage(currentChat, ex.Message);
        }
        catch (EmptyTaskListException ex)
        {
            botClient.SendMessage(currentChat, ex.Message);
        }
        catch (Exception ex)
        {
            botClient.SendMessage(currentChat, $"Произошла непредвиденная ошибка");
            throw;
        }

        #region МЕТОДЫ КОМАНД
        ////метод команд ShowTasks и ShowAllTasks
        void ShowTasks(Guid userId, bool isActive = false, IReadOnlyList<ToDoItem>? tasks = null)
        {
            //присвою список через оператор null объединения 
            var tasksList = tasks ?? (isActive 
                ? _toDoService.GetAllByUserId(userId) 
                : _toDoService.GetActiveByUserId(userId));

            if (tasksList.Count == 0) 
            {
                string emptyMessage = isActive ? "Список задач пуст.\n" : "Aктивных задач нет";
                botClient.SendMessage(currentChat, emptyMessage);
                return;
            }

            //выберу текст меседжа через тернарный оператор
            string message = tasks != null ? "Список найденных задач:"
                : (isActive ? "Список всех задач:" : "Список активных задач:");

            botClient.SendMessage(currentChat, message);

            Helper.TasksListRender(tasksList, botClient, currentChat);
        }

        ////метод команды Help
        void ShowHelp(ToDoUser user)
        {
            if (user == null)
            {
                botClient.SendMessage(currentChat, $"Незнакомец, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n");
            }
            else
            {
                botClient.SendMessage(currentChat, $"{user.TelegramUserName}, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/addtask\" *название задачи*\" ты сможешь добавлять задачи в список задач\n" +
                $"Введя команду \"/showtasks\" ты сможешь увидеть список активных задач в списке\n" +
                $"Введя команду \"/showalltasks\" ты сможешь увидеть список всех задач в списке\n" +
                $"Введя команду \"/removetask\" *номер задачи*\" ты сможешь удалить задачу из списка задач\n" +
                $"Введя команду \"/completetask\" *номер задачи*\" ты сможешь отметить задачу из списка как завершенную\n" +
                $"Введя команду \"/find\" *название задачи*\" ты сможешь увидеть список всех задач начинающихся с названия задачи\n" +
                $"Введя команду \"/report\" ты получишь отчёт по задачам\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n");
            }
        }

        ////метод команды Info
        void ShowInfo()
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            botClient.SendMessage(currentChat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n");
        }
        #endregion
    }
}