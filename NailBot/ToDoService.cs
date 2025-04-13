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
        //создам экземляр update, чтобы передавать в методы
        private static Update newUpdate;
        public Update NewUpdate
        {
            get { return newUpdate; }
            set { newUpdate = value; }
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
        public void ShowHelp(string userName)
        {
            string name = string.IsNullOrWhiteSpace(userName) ? "Незнакомец" : userName;

            if (name == "Незнакомец")
            {
                Init.botClient.SendMessage(chat, $"{name}, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду /start бот предложит тебе ввести имя\n" +
                $"Введя команду /help ты получишь справку о командах\n" +
                $"Введя команду /info ты получишь информацию о версии программы\n" +
                $"Введя команду /exit бот попрощается и завершит работу\n");
            }
            else
            {
                Init.botClient.SendMessage(chat, $"{name}, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду /start бот предложит тебе ввести имя\n" +
                $"Введя команду /help ты получишь справку о командах\n" +
                $"Введя команду /addtask ты сможешь добавлять задачи в список задач\n" +
                $"Введя команду /showtasks ты сможешь увидеть список добавленных задач в список задач\n" +
                $"Введя команду /removetask ты сможешь удалять задачи из списка задач\n" +
                $"Введя команду /info ты получишь информацию о версии программы\n" +
                $"Введя команду /exit бот попрощается и завершит работу\n");
            }
        }

        ////метод команды Info
        public void ShowInfo()
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            Init.botClient.SendMessage(chat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n");
        }

        //////метод рендера списка задач
        public void ShowTasks()
        {
            if (Init.tasksList.Count == 0)
                Init.botClient.SendMessage(chat, $"Список задач пуст.\n");
            else
            {
                Init.botClient.SendMessage(chat, $"\nСписок задач:");
                int taskCounter = 0;
                foreach (ToDoItem task in Init.tasksList)
                {
                    if (task.State != ToDoItemState.Active)
                        continue;

                    taskCounter++;
                    Init.botClient.SendMessage(chat, $"{taskCounter}) {task.Name} - {task.CreatedAt} - {task.Id}");
                }
            }
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
                return;
            

            //достану удаляемый объект задачи
            var removedProducts = Init.tasksList.Where(x => x.Id == id).ToList();

            //удаляю задачу
            Init.tasksList.RemoveAll(x => x.Id == id);

            Init.botClient.SendMessage(chat, $"Задача {removedProducts[0].Name} удалена.\n");

            ShowTasks();
        }

        //метод проверки корректного ввода команд /addtask и /removetask
        public (string, string, Guid) CheckAddAndRemove(string input)
        {
            string cutInput = "";

            Guid taskGuid = Guid.Empty;

            if (input.StartsWith("/addtask") || input.StartsWith("/removetask"))
            {
                if (input.StartsWith("/addtask "))
                {

                    cutInput = input.Substring(9);
                    input = "/addtask";
                }

                else if (input.StartsWith("/removetask "))
                {

                    //перенес проверку на длину списка задач сюда
                    if (Init.tasksList.Count == 0)
                    {
                        Init.botClient.SendMessage(chat, $"Ваш список задач пуст, удалять нечего.\n");
                        input = "/removetask";
                        return (input, cutInput, taskGuid);
                    }

                    cutInput = input.Substring(12);
                    input = "/removetask";

                    bool success = int.TryParse(cutInput, out int taskNumber);

                    if (success && (taskNumber > 0 && taskNumber <= Init.tasksList.Count))
                        taskGuid = Init.tasksList[taskNumber - 1].Id;
                    else
                        throw new ArgumentException($"Введён некорректный номер задачи.\n");
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
                if (item.Name.Contains(newTask))
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
        public void MarkCompleted(Guid id)
        {
            // реализация
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