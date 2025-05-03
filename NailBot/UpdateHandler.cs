using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.ComponentModel.DataAnnotations;
using System.Threading.Channels;

namespace NailBot
{
    public enum Commands
    {
        Start = 1, Help, Info, Addtask, Showtasks, Showalltasks, Removetask, Completetask, Exit
    }

    internal class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        private readonly ToDoService toDoService;

        private readonly UserService userService;

        public UpdateHandler(IUserService iuserService, IToDoService itoDoService)
        {
            _userService = iuserService ?? throw new ArgumentNullException(nameof(iuserService));
            _toDoService = itoDoService ?? throw new ArgumentNullException(nameof(itoDoService));

            //явно приведу к типу
            toDoService = (ToDoService)itoDoService;
            userService = (UserService)iuserService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine(toDoService.Equals(_toDoService));
            try
            {
                //получаю текущего юзера
                var currentUser = _userService.GetUser(update.Message.From.Id);

                //передаю бота
                toDoService.BotClient = botClient;

                //тут запрашиваю начальные ограничения длины задачи и их количества
                if (update.Message.Id == 1)
                {
                    //передаю в newService созданный экземпляр Chat
                    toDoService.Chat = update.Message.Chat;

                    toDoService.CheckMaxAmount(toDoService.Chat);

                    toDoService.CheckMaxLength(toDoService.Chat);

                    botClient.SendMessage(update.Message.Chat, $"Привет! Это Todo List Bot! Введите команду для начала работы или выхода из бота.\n");

                    Commands.Start.CommandsRender(currentUser, update, botClient);

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
                        Commands.Start.CommandsRender(currentUser, update, botClient);
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
                        botClient.SendMessage(update.Message.Chat, "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n");
                        Commands.Start.CommandsRender(currentUser, update, botClient);
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