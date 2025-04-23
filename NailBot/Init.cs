using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using static NailBot.Extensions;
using static NailBot.StartValues;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace NailBot
{
    public class Init
    {
        //создаю экземпляр бота
        public static ConsoleBotClient botClient = new ConsoleBotClient();
        public static ITelegramBotClient telegramBotClient = new ConsoleBotClient();


        //создаю экземпляр IUserService
        public static IUserService iuserService = new UserService();

        //создаю экземпляр IToDoService
        public static IToDoService itoDoService = new ToDoService();

        //создаю экземпляр объекта хендлера
        internal UpdateHandler handler = new UpdateHandler(iuserService, itoDoService);

        //создаю экземпляр объекта апдейт
        public Update update = new Update();

        public void Start()
        {
            //запуск бота
            botClient.StartReceiving(handler);
        }

        public static void Stop()
        {
            //userName = userName != "Пользователь" ? userName : "Незнакомец";
            Console.WriteLine();
      
            //botClient.SendMessage(update.Message.Chat, $"До свидания, {userName}! До новых встреч!");
            Console.ReadKey();
            return;
        }
    }
}

//Расширение функционала приложения, разработанного в предыдущем домашнем задании:

//Добавление интерфейсов и классов аналогичных Telegram API, чтобы в будущем было легче переключиться на реального Telegram бота
//Работа с классами и интерфейсами
//Добавление новых команд

//Описание/Пошаговая инструкция выполнения домашнего задания:
//Ссылка на описание в GitHub

//Перед выполнением нужно ознакомится с Правила отправки домашнего задания на проверку

//Подключение библиотеки Otus.ToDoList.ConsoleBot
//Добавить к себе в решение и в зависимости к своему проекту с ботом проект Otus.ToDoList.ConsoleBot GitHub
//Ознакомиться с классами в папке Types и с README.md
//Создать класс UpdateHandler, который реализует интерфейс IUpdateHandler, и перенести в метод HandleUpdateAsync обработку всех команд. Вместо Console.WriteLine использовать SendMessage у ITelegramBotClient
//Перенести try/catch в HandleUpdateAsync. В Main оставить catch(Exception)
//Для вывода в коноль сообщений использовать метод ITelegramBotClient.SendMessage
//Удалить команду /echo
//Изменение логики команды /start
//Не нужно запрашивать имя
//Добавить класс User
//Свойства
//Guid UserId
//long TelegramUserId
//string TelegramUserName
//DateTime RegisteredAt
//Добавление класса сервиса UserService
//Добавить интерфейс IUserService
//interface IUserService
//{
//    User RegisterUser(long telegramUserId, string telegramUserName);
//    User? GetUser(long telegramUserId);
//}
//Создать класс UserService, который реализует интерфейс IUserService. Заполнять telegramUserId и telegramUserName нужно из значений Update.Message.From
//Добавить использование IUserService в UpdateHandler. Получать IUserService нужно через конструктор
//При команде /start нужно вызвать метод IUserService.RegisterUser.
//Если пользователь не зарегистрирован, то ему доступны только команды /help /info
//Добавление класса ToDoItem
//Добавить enum ToDoItemState с двумя значениями
//Active
//Completed
//Добавить класс ToDoItem
//Свойства
//Guid Id
//User User
//string Name
//DateTime CreatedAt
//ToDoItemState State
//DateTime? StateChangedAt - обновляется при изменении State
//Добавить использование класса ToDoItem вместо хранения только имени задачи
//Изменение логики /showtasks
//Выводить только задачи с ToDoItemState.Active
//Добавить вывод CreatedAt и Id. Пример: Имя задачи - 01.01.2025 00:00:00 - 17056344 - 0e03 - 4a21 - b0dd - f0d30a5abf49
//Добавление класса сервиса ToDoService
//Добавить интерфейс IToDoService
//public interface IToDoService
//{
//    //Возвращает ToDoItem для UserId со статусом Active
//    IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
//    ToDoItem Add(User user, string name);
//    void MarkCompleted(Guid id);
//    void Delete(Guid id);
//}
//Создать класс ToDoService, который реализует интерфейс IToDoService. Перенести в него логику обработки команд
//Добавить использование IToDoService в UpdateHandler. Получать IToDoService нужно через конструктор
//Изменить формат обработки команды /addtask. Нужно сразу передавать имя задачи в команде. Пример: / addtask Новая задача
//Изменить формат обработки команды /removetask. Нужно сразу передавать номер задачи в команде. Пример: / removetask 2
//Добавление команды /completetask
//Добавить обработку новой команды /completetask. При обработке команды использовать метод IToDoService.MarkAsCompleted
//Пример: / completetask 73c7940a - ca8c - 4327 - 8a15 - 9119bffd1d5e
//Добавление команды /showalltasks
//Добавить обработку новой команды /showalltasks. По ней выводить команды с любым State и добавить State в вывод
//Пример: (Active)Имя задачи - 01.01.2025 00:00:00 - ffbfe448 - 4b39 - 4778 - 98aa - 1aed98f7eed8
//Обновить /help

//Критерии оценки:
//Пункты 1-7 - 8 баллов
//Пункт 8 - 1 балл
//Пункты 9-10 - 1 балл
//Для зачёта домашнего задания достаточно 8 баллов.