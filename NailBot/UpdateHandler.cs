using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

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
        UserService userService = new UserService();

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                //вызов метода обработки команд
                toDoService.CommandsHandle(_userService, toDoService, _toDoService, update);

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