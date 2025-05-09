using NailBot.Core.Exceptions;
using NailBot.Core.Services;
using NailBot.Helpers;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace NailBot.TelegramBot
{
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
            var currentChat = toDoService.GetChat(update);
            try
            {
                //получаю текущего юзера
                var currentUser = _userService.GetUser(update.Message.From.Id);           

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
                (string inputCommand, string inputText, Guid taskGuid) inputs = toDoService.InputCheck(input, currentUser);

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
                        toDoService.ShowTasks(currentUser.UserId);
                        break;

                    case Commands.Showalltasks:
                        //вызов метода рендера задач
                        toDoService.ShowTasks(currentUser.UserId, true);
                        break;

                    case Commands.Removetask:
                        //вызов метода удаления задачи
                        _toDoService.Delete(inputs.taskGuid);
                        break;

                    case Commands.Completetask:
                        //вызов метода выполнения задачи
                        _toDoService.MarkCompleted(inputs.taskGuid);
                        break;

                    case Commands.Find:
                        //вызов метода поиска задачи
                        _toDoService.Find(currentUser, inputs.inputText);
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
            catch (Exception ex)
            {
                botClient.SendMessage(currentChat, $"Произошла непредвиденная ошибка");
                throw;
            }
            return;
        }
    }
}