using NailBot;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    class ToDoService : IToDoService
    {
        //создам экземляр чата, чтобы передавать в SendMessage
        private static Chat chat;
        public Chat Chat {
            get { return chat; }
            set { chat = value; }
        }
        //чтобы заполнить User1 типа User создал статический экземляр Update внутри UpdateHandler
        private static Update updateForUser1;
        public Update Uppp
        {
            get { return updateForUser1; }
            set { updateForUser1 = value; }
        }

        ////старый метод создания задачи
        //public void AddTask()
        //{
        //}

        //////старый метод удаления задачи
        //public void RemoveTask(string removeTask)
        //{
        //}

        ////МЕТОДЫ КОМАНД
        ////метод команды Help
        public void ShowHelp(ToDoUser user)
        {
            if (user.TelegramUserName == null)
            {
                Init.botClient.SendMessage(chat, $"Незнакомец, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду \"/start\" бот предложит тебе ввести имя\n" +
                $"Введя команду \"/help\" ты получишь справку о командах\n" +
                $"Введя команду \"/info\" ты получишь информацию о версии программы\n" +
                $"Введя команду \"/exit\" бот попрощается и завершит работу\n");
            }
            else
            {
                Init.botClient.SendMessage(chat, $"{user.TelegramUserName}, это Todo List Bot - телеграм бот записи дел.\n" +
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
            Init.botClient.SendMessage(chat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n");
        }

        //////метод рендера списка активных задач 
        ///добавлю параметр по умолчанию для команды showalltasks и буду передавать в него true
        public void ShowTasks(bool allTasks = false)
        {
            if (Init.tasksList.Count == 0)
                Init.botClient.SendMessage(chat, $"Список задач пуст.\n");
            else
            {
                int taskCounter = 0;

                if (!allTasks)
                {
                    //проверяю есть ли активные задачи
                    var completedTasks = Init.tasksList.Where(x => x.State == ToDoItemState.Active).ToList();

                    if (completedTasks.Count > 0)
                    {
                        Init.botClient.SendMessage(chat, $"Список активных задач:");
                        foreach (ToDoItem task in Init.tasksList)
                        {
                            if (task.State != ToDoItemState.Active)
                                continue;

                            taskCounter++;
                            Init.botClient.SendMessage(chat, $"{taskCounter}) {task.Name} - {task.CreatedAt} - {task.Id}");
                        }
                    }
                    else
                        Init.botClient.SendMessage(chat, $"Активных задач нет");
                }        
                else
                {
                    Init.botClient.SendMessage(chat, $"Список всех задач:");
                    foreach (ToDoItem task in Init.tasksList)
                    {
                        taskCounter++;
                        Init.botClient.SendMessage(chat, $"{taskCounter}) ({(task.State)}) {task.Name} - {task.CreatedAt} - {task.Id}");
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
            if (Init.tasksList.Count >= Init.maxTaskAmount)
                throw new TaskCountLimitException(Init.maxTaskAmount);

            //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
            string newTask = Validate.ValidateString(name, Init.maxTaskLenght);

            //проверяю дубликаты введённой задачи
            CheckDuplicate(newTask);

            ToDoItem newToDoItem = new ToDoItem
            {

                //чтобы заполнить User1 типа User создал статический экземляр Update внутри UpdateHandler
                User1 = updateForUser1.Message.From,


                Name = newTask,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                User = user,
                StateChangedAt = DateTime.Now,
            };

            Init.tasksList.Add(newToDoItem);

            Init.botClient.SendMessage(chat, $"Задача \"{newTask}\" добавлена в список задач.\n");

            //возвращаю новый объект задачи
            return newToDoItem;
        }

        // реализация метода интерфейса Delete
        public void Delete(Guid id)
        {
            //проверка на наличие переданной задачи
            if (id == Guid.Empty)
            {
                Init.botClient.SendMessage(chat, $"Ваш список задач пуст, удалять нечего.\n");
                return;
            }   
            
            //достану удаляемый объект задачи
            var removedProducts = Init.tasksList.Where(x => x.Id == id).ToList();

            //удаляю задачу
            Init.tasksList.RemoveAll(x => x.Id == id);

            Init.botClient.SendMessage(chat, $"Задача {removedProducts[0].Name} удалена.\n");

            ShowTasks();
        }

        // реализация метода интерфейса MarkCompleted
        public void MarkCompleted(Guid id)
        {
            //проверка на наличие переданной задачи
            if (id == Guid.Empty)
            {
                Init.botClient.SendMessage(chat, $"Ваш список задач пуст, выполнять нечего.\n");
                return;
            }

            //найду в списке необходимую задачу
            var completedTask = Init.tasksList.FirstOrDefault(u => u.Id == id);

            if (completedTask != null)
                //выполню её
                completedTask.State = ToDoItemState.Completed;

            Init.botClient.SendMessage(chat, $"Задача {completedTask.Name} удалена.\n");
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
                    (string command, string inputText, Guid taskGuid) inputData = Validate.ValidateTask(input, cutInput, taskGuid);

                    input = inputData.command;
                    cutInput = inputData.inputText;
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
            foreach (var item in Init.tasksList)
            {
                if (item.Name == newTask)
                    throw new DuplicateTaskException(newTask);
            }           
        }

        //валидация начальных значений задач
        public int CheckMaxAmount(Chat chat)
        {
            //присваиваю значения длин
            if (Init.maxTaskAmount == 0)
                Init.maxTaskAmount = Init.maxTaskAmount.GetStartValues("Введите максимально допустимое количество задач", chat);

            return Init.maxTaskAmount;
        }
        public int CheckMaxLength(Chat chat)
        {
            //присваиваю значения длин
            if (Init.maxTaskLenght == 0)
                Init.maxTaskLenght = Init.maxTaskLenght.GetStartValues("Введите максимально допустимую длину задачи", chat);

            return Init.maxTaskLenght;
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


//Добавление команды /completetask
//Добавить обработку новой команды /completetask. При обработке команды использовать метод IToDoService.MarkAsCompleted
//Пример: / completetask 73c7940a - ca8c - 4327 - 8a15 - 9119bffd1d5e
//Добавление команды /showalltasks
//Добавить обработку новой команды /showalltasks. По ней выводить команды с любым State и добавить State в вывод
//Пример: (Active)Имя задачи - 01.01.2025 00:00:00 - ffbfe448 - 4b39 - 4778 - 98aa - 1aed98f7eed8
//Обновить /help