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
        private Chat chat;
        public Chat Chat {
            get { return chat; }
            set { chat = value; }
        }

        private Update newUpdate;
        public Update NewUpdate
        {
            get { return newUpdate; }
            set { newUpdate = value; }
        }


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


        ////работа с List
        ////метод добавления задачи в List
        //public void AddTaskList()
        //{
        //}

        //////метод удаления задачи из List
        //public void RemoveTaskList(List<ToDoItem> tasks, string removeTask)
        //{
        //}

        //////метод рендера задач из List
        public void ShowTasks()
        {
            if (Init.tasksList.Count == 0)
                Init.botClient.SendMessage(chat, $"Список задач из List пуст.\n");
            else
            {
                Init.botClient.SendMessage(chat, $"\nСписок задач из List:");
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

        // реализация метода интерфейса Add для List
        public ToDoItem Add(ToDoUser user, string name)
        {
            //проверяю длину листа и выбрасываю исключение если больше лимита
            if (Init.tasksList.Count >= Init.maxTaskAmount)
                throw new TaskCountLimitException(Init.maxTaskAmount);

            //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
            string newTask = Validate.ValidateString(name, Init.maxTaskLenght);

            //проверяю дубликаты введённой задачи
            CheckDuplicateList(Init.tasksList, newTask);

            ToDoItem newToDoItem = new ToDoItem
            {
                Name = newTask,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                User = user,
                StateChangedAt = DateTime.Now,
            };

            Init.tasksList.Add(newToDoItem);

            Init.botClient.SendMessage(chat, $"Задача \"{newTask}\" добавлена в List задач.\n");

            //возвращаю новый объект задачи
            return newToDoItem;
        }


        // реализация метода интерфейса Delete для List
        public void Delete(Guid id)
        {
            if (Init.tasksList.Count == 0)
            {
                Init.botClient.SendMessage(chat, $"Ваш список задач пуст, удалять нечего.\n");

                ShowTasks();

                return;
            }

            //достану удаляемый объект задачи
            var removedProducts = Init.tasksList.Where(x => x.Id == id).ToList();

            //удаляю задачу
            Init.tasksList.RemoveAll(x => x.Id == id);

            Init.botClient.SendMessage(chat, $"Задача {removedProducts[0].Name} удалена.\n");

            ShowTasks();
        }

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

                    //перенесу проверку на пустоту списка с объектами задачи
                    if (Init.tasksList.Count == 0)
                    {
                        Init.botClient.SendMessage(chat, $"Ваш список задач пуст, удалять нечего.\n");

                        ShowTasks();

                        return (input, cutInput, taskGuid);
                    }

                    cutInput = input.Substring(12);
                    input = "/removetask";


                    bool success = int.TryParse(cutInput, out int taskNumber);

                    if (success && (taskNumber > 0 && taskNumber <= Init.tasksList.Count))
                        taskGuid = Init.tasksList[taskNumber - 1].Id;
                    else
                    {
                        throw new ArgumentException($"Введён некорректный номер задачи.\n");

                        //RemoveTaskList(tasks, removeTask);
                    }



                }
                else
                {
                    input = "unregistered user command";
                }
            }
            return (input, cutInput, taskGuid);
        }



        //////метод удаления задачи из Array
        public void RemoveTaskArray(ref ToDoItem[] tasks, string removeTask)
        {
            if (tasks.Length == 0)
            {
                Init.botClient.SendMessage(chat, $"Ваш список задач пуст, удалять нечего.\n");
                return;
            }

            ShowTasks();

            bool success = int.TryParse(removeTask, out int taskNumber);

            if (success && (taskNumber > 0 && taskNumber <= tasks.Length))
            {
                Init.botClient.SendMessage(chat, $"Задача {tasks[taskNumber - 1].Name} удалена.\n");

                ToDoItem[] newTasks = new ToDoItem[tasks.Length - 1];

                int index = taskNumber;

                for (int i = 0; i < index - 1; i++)
                    newTasks[i] = tasks[i];

                for (int i = index - 1; i < newTasks.Length; i++)
                    newTasks[i] = tasks[i + 1];

                tasks = newTasks;

                ShowTasks();
            }
            else
            {
                Init.botClient.SendMessage(chat, $"Введён некорректный номер задачи.\n");
                return;
                //RemoveTaskArray(ref tasks);
            }
        }




        //проверка дубликатов Arr
        void CheckDuplicateArr(ToDoItem[] tasks, string newTask)
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                if (tasks[i].Name == newTask)
                    throw new DuplicateTaskException(newTask);
            }
        }

        //проверка дубликатов List
        void CheckDuplicateList(List<ToDoItem> tasks, string newTask)
        {
            foreach (var item in tasks)
            {
                if (item.Name.Contains(newTask))
                    throw new DuplicateTaskException(newTask);
            }           
        }

        //проверка корректности названия новой задачи и номера удаляемой
        //public (string, string, Guid) CheckAddAndRemove(string input)
        //{
        //    string cutInput = "";
        //    Guid taskGuid = Guid.Empty;
        //    if (input.StartsWith("/addtask") || input.StartsWith("/removetask"))
        //    {
        //        if (input.StartsWith("/addtask "))
        //        {
        //            cutInput = input.Substring(9);
        //            input = "/addtask";
        //        }
                
        //        else if (input.StartsWith("/removetask "))
        //        {
        //            cutInput = input.Substring(12);
        //            input = "/removetask";

        //            //task = int.Parse(cutInput.ToString());
        //        }
        //        else
        //        {
        //            input = "unregistered user command";
        //        }
        //    }
        //    return (input, cutInput, taskGuid);
        //}


        //валидация начальных значений задач
        public int CheckMaxAmount(int maxAmount, Chat chat)
        {
            //присваиваю значения длин
            if (maxAmount == 0)
                maxAmount = maxAmount.GetStartValues("Введите максимально допустимое количество задач", chat);

            return maxAmount;
        }

        public int CheckMaxLength(int maxLength, Chat chat)
        {
            //присваиваю значения длин
            if (maxLength == 0)
                maxLength = maxLength.GetStartValues("Введите максимально допустимую длину задачи", chat);

            return maxLength;
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




        // реализация метода Add для Array
        //public ToDoItem Add(ToDoUser user, string name)
        //{
        //    ToDoItem[] arrayTasks = new ToDoItem[Init.tasksArray.Length + 1];

        //    //проверяю длину  и выбрасываю исключение если больше лимита
        //    if (Init.tasksArray.Length >= Init.maxTaskAmount)
        //        throw new TaskCountLimitException(Init.maxTaskAmount);

        //    //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
        //    string newTask = Validate.ValidateString(name, Init.maxTaskLenght);

        //    int index = arrayTasks.Length - 1;

        //    //проверяю дубликаты введённой задачи
        //    CheckDuplicateArr(Init.tasksArray, newTask);

        //    //инициализирую новый объект задачи
        //    arrayTasks[index] = new ToDoItem
        //    {
        //        Name = newTask,
        //        Id = Guid.NewGuid(),
        //        CreatedAt = DateTime.Now,
        //        User = user,
        //        StateChangedAt = DateTime.Now,
        //    };

        //    for (int i = 0; i < index; i++)
        //        arrayTasks[i] = Init.tasksArray[i];

        //    Init.tasksArray = arrayTasks;

        //    Init.botClient.SendMessage(chat, $"Задача \"{newTask}\" добавлена в Array задач.\n");

        //    return arrayTasks[index];
        //}


        ////работа с Array
        ////метод добавления задачи в Array 
        //public void AddTaskArray(ToDoUser user, string taskText)
        //{
        //}

        //////метод рендера задач из Array
        //public void ShowTasks()
        //{
        //    if (Init.tasksArray.Length == 0)
        //        Init.botClient.SendMessage(chat, $"Список задач из Array пуст.\n");
        //    else
        //    {
        //        Init.botClient.SendMessage(chat, $"\nСписок задач из Array:");
        //        for (int i = 0; i < Init.tasksArray.Length; i++)
        //            Init.botClient.SendMessage(chat, $"{i + 1}) {Init.tasksArray[i].Name} - {Init.tasksArray[i].CreatedAt} - {Init.tasksArray[i].Id}");
        //    }
        //}



    }
}


//Добавление команды /completetask
//Добавить обработку новой команды /completetask. При обработке команды использовать метод IToDoService.MarkAsCompleted
//Пример: / completetask 73c7940a - ca8c - 4327 - 8a15 - 9119bffd1d5e
//Добавление команды /showalltasks
//Добавить обработку новой команды /showalltasks. По ней выводить команды с любым State и добавить State в вывод
//Пример: (Active)Имя задачи - 01.01.2025 00:00:00 - ffbfe448 - 4b39 - 4778 - 98aa - 1aed98f7eed8
//Обновить /help