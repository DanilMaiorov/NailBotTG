using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using NailBot.Core.Exceptions;
using NailBot.Helpers;
using NailBot.TelegramBot;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

namespace NailBot.Core.Services
{
    //заведу делегат
    //delegate IReadOnlyList<ToDoItem> ListGetter(Guid userId);

    class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;

        private readonly ITelegramBotClient _botClient;

        public ToDoService(IToDoRepository toDoRepository, ITelegramBotClient botClient)
        {
            _toDoRepository = toDoRepository;
            _botClient = botClient;

            maxTaskAmount = CheckMaxAmount();
            maxTaskLenght = CheckMaxLength();
        }

        //количество задач при запуске программы
        private int maxTaskAmount;

        //длина задачи при запуске программы
        private int maxTaskLenght;


        //создам экземляр чата, чтобы передавать в SendMessage
        public Chat Chat { get; set; }        


        //Возвращает IReadOnlyList<ToDoItem> для UserId
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId) => _toDoRepository.GetAllByUserId(userId);
        //Возвращает IReadOnlyList<ToDoItem> для UserId со статусом Active
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId) => _toDoRepository.GetActiveByUserId(userId);

        // реализация метода интерфейса Add
        public ToDoItem Add(ToDoUser user, string name)
        {
            var tasks = GetAllByUserId(user.UserId);

            //проверяю длину листа и выбрасываю исключение если больше лимита
            if (tasks.Count >= maxTaskAmount)
                throw new TaskCountLimitException(maxTaskAmount);

            //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
            string newTask = Validate.ValidateString(name, maxTaskLenght);

            //проверяю дубликаты введённой задачи
            CheckDuplicate(newTask, user);

            ToDoItem newToDoItem = new ToDoItem
            {
                Name = newTask,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                User = user,
                StateChangedAt = DateTime.Now,
            };

            //добавляю в репозиторий
            _toDoRepository.Add(newToDoItem);

            _botClient.SendMessage(Chat, $"Задача \"{newTask}\" добавлена в список задач.\n");

            //возвращаю новый объект задачи
            return newToDoItem;
        }

        // реализация метода интерфейса Delete
        public void Delete(Guid id)
        {

            var item = _toDoRepository.Get(id);

            if (item == null)
            {
                return;
            }

            var allTasks = GetAllByUserId(item.User.UserId);


            //проверка на наличие переданной задачи
            if (id == Guid.Empty)
            {
                _botClient.SendMessage(Chat, $"Ваш список задач пуст, удалять нечего.\n");
                return;
            }

            //достану удаляемый объект задачи
            var removedProducts = allTasks.FirstOrDefault(x => x.Id == id);

            //удаляю задачу
            _toDoRepository.Delete(id);

            _botClient.SendMessage(Chat, $"Задача {removedProducts.Name} удалена.\n");
        }

        // реализация метода интерфейса MarkCompleted
        public void MarkCompleted(Guid id)
        {
            var item = _toDoRepository.Get(id);

            if (item == null)
            {
                return;
            }

            var allTasks = GetAllByUserId(item.User.UserId);

            //проверка на наличие переданной задачи
            if (id == Guid.Empty)
            {
                _botClient.SendMessage(Chat, $"Ваш список задач пуст, выполнять нечего.\n");
                return;
            }

            //найду в списке необходимую задачу
            var completedTask = allTasks.FirstOrDefault(u => u.Id == id);

            if (completedTask != null)
                //выполню её
                completedTask.State = ToDoItemState.Completed;

            _botClient.SendMessage(Chat, $"Задача {completedTask.Name} выполнена.\n");
        }

        // реализация метода интерфейса Find
        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            var tasks = GetAllByUserId(user.UserId);

            if (tasks.Count == 0)
            {
                _botClient.SendMessage(Chat, $"Ваш список задач пуст, искать нечего.\n");
                return null;
            }

            bool isContain = _toDoRepository.ExistsByName(user.UserId, namePrefix);

            if (!isContain)
            {
                _botClient.SendMessage(Chat, $"Задач, начинающихся на {namePrefix} не найдено.\n");
                return null;
            }

            var findedTasks = _toDoRepository.Find(user.UserId, item => item.Name.Contains(namePrefix));

            ShowTasks(user.UserId, false, findedTasks);

            return findedTasks;
        }


        #region МЕТОДЫ КОМАНД
        ////метод команды Help
        public void ShowHelp(ToDoUser user)
        {
            if (user == null)
            {
                _botClient.SendMessage(Chat, $"Незнакомец, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n");
            }
            else
            {
                _botClient.SendMessage(Chat, $"{user.TelegramUserName}, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/addtask\" *название задачи*\" ты сможешь добавлять задачи в список задач\n" +
                $"Введя команду \"/showtasks\" ты сможешь увидеть список активных задач в списке\n" +
                $"Введя команду \"/showalltasks\" ты сможешь увидеть список всех задач в списке\n" +
                $"Введя команду \"/removetask\" *номер задачи*\" ты сможешь удалить задачу из списка задач\n" +
                $"Введя команду \"/completetask\" *номер задачи*\" ты сможешь отметить задачу из списка как завершенную\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n");
            }
        }

        ////метод команды Info
        public void ShowInfo()
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            _botClient.SendMessage(Chat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n");
        }

        ////метод рендера списка задач 
        ///добавлю параметр по умолчанию для команды showalltasks и буду передавать в него true
        public void ShowTasks(Guid userId, bool isActive = false, IReadOnlyList<ToDoItem>? tasks = null)
        {
            //присвою список через оператор null объединения 
            IReadOnlyList<ToDoItem> tasksList = tasks ?? (isActive ? GetAllByUserId(userId) : GetActiveByUserId(userId));

            if (tasksList.Count == 0)
            {
                string emptyMessage = isActive ? "Список задач пуст.\n" : "Aктивных задач нет";
                _botClient.SendMessage(Chat, emptyMessage);
                return;
            }

            //выберу текст меседжа через тернарный оператор
            string message = tasks != null ? "Список найденных задач:"
                : (isActive ? "Список всех задач:" : "Список активных задач:");

            _botClient.SendMessage(Chat, message);
            TasksListRender(tasksList);
        }
        #endregion


        #region Вспомогательные методы
        //ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        //метод проверки корректного ввода команд /addtask и /removetask
        public (string, string, Guid) InputCheck(string input, ToDoUser currentUser)
        {
            


            string cutInput = "";

            Guid taskGuid = Guid.Empty;

            if (input.StartsWith("/addtask") || input.StartsWith("/removetask") || input.StartsWith("/completetask") || input.StartsWith("/find"))
            {
                if (input.StartsWith("/addtask "))
                {
                    cutInput = input.Substring(9);
                    input = "/addtask";
                } 
                else if (input.StartsWith("/find "))
                {
                    cutInput = input.Substring(6);
                    input = "/find";
                }
                else if (input.StartsWith("/removetask ") || input.StartsWith("/completetask "))
                {
                    var allTasks = GetAllByUserId(currentUser.UserId);
                    //верну данные кортежем
                    (string command, Guid taskGuid) inputData = Validate.ValidateTask(input, taskGuid, allTasks);

                    input = inputData.command;
                    taskGuid = inputData.taskGuid;
                }
                else
                    input = "unregistered user command";
            }
            return (input, cutInput, taskGuid);
        }

        //проверка дубликатов
        void CheckDuplicate(string newTask, ToDoUser currentUser)
        {
            if (GetAllByUserId(currentUser.UserId).Any(item => item.Name == newTask))
            {
                throw new DuplicateTaskException(newTask);
            }
                
        }

        //валидация начальных значений задач
        public int CheckMaxAmount()
        {
            return maxTaskAmount.GetStartValues("Введите максимально допустимое количество задач");
        } 
        
        //валидация начальных значений задач
        public int CheckMaxLength()
        {
            return maxTaskLenght.GetStartValues("Введите максимально допустимую длину задачи");
        }
            
        
        //рендер списк задач
        private void TasksListRender(IReadOnlyList<ToDoItem> tasks)
        {
            int taskCounter = 0;

            foreach (ToDoItem task in tasks)
            {
                taskCounter++;
                _botClient.SendMessage(Chat, $"{taskCounter}) ({task.State}) {task.Name} - {task.CreatedAt} - {task.Id}");
            }
        }
        #endregion
    }
}
