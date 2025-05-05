using NailBot;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    class ToDoService : IToDoService
    {
        //объявление списка задач в виде List
        public List<ToDoItem> TasksList = new List<ToDoItem>();

        //количество задач при запуске программы
        private int maxTaskAmount;
        public int MaxTaskAmount
        {
            get { return maxTaskAmount; }
            set { maxTaskAmount = value; }
        }

        //длина задачи при запуске программы
        private int maxTaskLenght;
        public int MaxTaskLenght
        {
            get { return maxTaskLenght; }
            set { maxTaskLenght = value; }
        }

        //создам экземляр чата, чтобы передавать в SendMessage
        private static Chat chat;
        public Chat Chat {
            get { return chat; }
            set { chat = value; }
        }

        //бот для сообщений
        private ITelegramBotClient _botClient;
        public ITelegramBotClient BotClient
        {
            get { return _botClient; }
            set { _botClient = value; }
        }

        ////МЕТОДЫ КОМАНД
        ////метод команды Help
        public void ShowHelp(ToDoUser user)
        {
            if (user == null)
            {
                _botClient.SendMessage(chat, $"Незнакомец, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n");
            }
            else
            {
                _botClient.SendMessage(chat, $"{user.TelegramUserName}, это Todo List Bot - телеграм бот записи дел.\n" +
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
            _botClient.SendMessage(chat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n");
        }

        //////метод рендера списка активных задач 
        ///добавлю параметр по умолчанию для команды showalltasks и буду передавать в него true
        public void ShowTasks(bool allTasks = false)
        {
            if (TasksList.Count == 0)
                _botClient.SendMessage(chat, $"Список задач пуст.\n");
            else
            {
                int taskCounter = 0;

                if (!allTasks)
                {
                    //проверяю есть ли активные задачи
                    var completedTasks = TasksList.Where(x => x.State == ToDoItemState.Active).ToList();

                    if (completedTasks.Count > 0)
                    {
                        _botClient.SendMessage(chat, $"Список активных задач:");
                        foreach (ToDoItem task in TasksList)
                        {
                            if (task.State != ToDoItemState.Active)
                                continue;

                            taskCounter++;
                            _botClient.SendMessage(chat, $"{taskCounter}) {task.Name} - {task.CreatedAt} - {task.Id}");
                        }
                    }
                    else
                        _botClient.SendMessage(chat, $"Активных задач нет");
                }        
                else
                {
                    _botClient.SendMessage(chat, $"Список всех задач:");
                    foreach (ToDoItem task in TasksList)
                    {
                        taskCounter++;
                        _botClient.SendMessage(chat, $"{taskCounter}) ({(task.State)}) {task.Name} - {task.CreatedAt} - {task.Id}");
                    }
                }
            }
        }

        //метод рендера всего списка задач
        public void ShowAllTasks()
        {
            ShowTasks(true);
        }

        // реализация метода интерфейса Add
        public ToDoItem Add(ToDoUser user, string name)
        {
            //проверяю длину листа и выбрасываю исключение если больше лимита
            if (TasksList.Count >= maxTaskAmount)
                throw new TaskCountLimitException(maxTaskAmount);

            //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
            string newTask = Validate.ValidateString(name, maxTaskLenght);

            //проверяю дубликаты введённой задачи
            CheckDuplicate(newTask);

            ToDoItem newToDoItem = new ToDoItem
            {
                Name = newTask,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                User = user,
                StateChangedAt = DateTime.Now,
            };

            TasksList.Add(newToDoItem);

            _botClient.SendMessage(chat, $"Задача \"{newTask}\" добавлена в список задач.\n");

            //возвращаю новый объект задачи
            return newToDoItem;
        }

        // реализация метода интерфейса Delete
        public void Delete(Guid id)
        {
            //проверка на наличие переданной задачи
            if (id == Guid.Empty)
            {
                _botClient.SendMessage(chat, $"Ваш список задач пуст, удалять нечего.\n");
                return;
            }   
            
            //достану удаляемый объект задачи
            var removedProducts = TasksList.Where(x => x.Id == id).ToList();

            //удаляю задачу
            TasksList.RemoveAll(x => x.Id == id);

            _botClient.SendMessage(chat, $"Задача {removedProducts[0].Name} удалена.\n");

            ShowTasks();
        }

        // реализация метода интерфейса MarkCompleted
        public void MarkCompleted(Guid id)
        {
            //проверка на наличие переданной задачи
            if (id == Guid.Empty)
            {
                _botClient.SendMessage(chat, $"Ваш список задач пуст, выполнять нечего.\n");
                return;
            }

            //найду в списке необходимую задачу
            var completedTask = TasksList.FirstOrDefault(u => u.Id == id);

            if (completedTask != null)
                //выполню её
                completedTask.State = ToDoItemState.Completed;

            _botClient.SendMessage(chat, $"Задача {completedTask.Name} выполнена.\n");
        }

        //метод проверки корректного ввода команд /addtask и /removetask
        public (string, string, Guid) InputCheck(string input)  
        {
            string cutInput = "";

            Guid taskGuid = Guid.Empty;

            if (input.StartsWith("/addtask") || input.StartsWith("/removetask") || input.StartsWith("/completetask"))
            {
                if (input.StartsWith("/addtask "))
                {
                    cutInput = input.Substring(9);
                    input = "/addtask";
                }
                else if (input.StartsWith("/removetask ") || input.StartsWith("/completetask "))
                {
                    //верну данные кортежем
                    (string command, Guid taskGuid) inputData = Validate.ValidateTask(input, taskGuid, TasksList);

                    input = inputData.command;
                    //cutInput = inputData.inputText;
                    taskGuid = inputData.taskGuid;
                }
                else
                    input = "unregistered user command";
            }
            return (input, cutInput, taskGuid);
        }

        //проверка дубликатов
        void CheckDuplicate(string newTask)
        {
            foreach (var item in TasksList)
            {
                if (item.Name == newTask)
                    throw new DuplicateTaskException(newTask);
            }           
        }

        //валидация начальных значений задач
        public void CheckMaxAmount(Chat chat)
        {
            MaxTaskAmount = maxTaskAmount.GetStartValues("Введите максимально допустимое количество задач", chat, _botClient);
        }
        public void CheckMaxLength(Chat chat)
        {
            MaxTaskLenght = maxTaskLenght.GetStartValues("Введите максимально допустимую длину задачи", chat, _botClient);
        }

        //ДЛЯ ДАЛЬНЕЙШЕЙ РАБОТЫ
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            var list = new List<ToDoItem>();

            return list;
        }
        //Возвращает ToDoItem для UserId со статусом Active
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            var list = new List<ToDoItem>();

            return list;
        }
    }
}