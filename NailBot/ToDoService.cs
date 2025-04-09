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
        ////МЕТОДЫ КОМАНД
        ////метод команды Help
        public void ShowHelp(string userName, ITelegramBotClient botClient, Update update)
        {
            string name = string.IsNullOrWhiteSpace(userName) ? "Незнакомец" : userName;

            if (name == "Незнакомец")
            {
                botClient.SendMessage(update.Message.Chat, $"{name}, это Todo List Bot - телеграм бот записи дел.\n" +
                $"Введя команду /start бот предложит тебе ввести имя\n" +
                $"Введя команду /help ты получишь справку о командах\n" +
                $"Введя команду /info ты получишь информацию о версии программы\n" +
                $"Введя команду /exit бот попрощается и завершит работу\n");
            }
            else
            {
                botClient.SendMessage(update.Message.Chat, $"{name}, это Todo List Bot - телеграм бот записи дел.\n" +
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
        public void ShowInfo(ITelegramBotClient botClient, Update update)
        {
            DateTime releaseDate = new DateTime(2025, 02, 08);
            botClient.SendMessage(update.Message.Chat, $"Это NailBot версии 1.0 Beta. Релиз {releaseDate}.\n");
        }

        ////работа с List
        ////метод добавления задачи в List
        public void AddTaskList(ToDoUser user, string taskText, ITelegramBotClient botClient, Update update)
        {
            //проверяю длину листа и выбрасываю исключение если больше лимита
            if (Init.tasksList.Count >= Init.maxTaskAmount)
                throw new TaskCountLimitException(Init.maxTaskAmount);

            //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
            string newTask = Validate.ValidateString(taskText, Init.maxTaskLenght);

            //проверяю дубликаты введённой задачи
            CheckDuplicateList(Init.tasksList, newTask);

            //инициализирую новый объект задачи
            var newItem = new ToDoItem
            {
                Name = newTask,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                User = user,
                StateChangedAt = DateTime.Now,
            };

            Init.tasksList.Add(newItem);

            botClient.SendMessage(update.Message.Chat, $"Задача \"{newTask}\" добавлена в List задач.\n");
        }

        //////метод рендера задач из List
        public void ShowTasks(List<ToDoItem> tasks, ITelegramBotClient botClient, Update update)
        {
            if (tasks.Count == 0)
                botClient.SendMessage(update.Message.Chat, $"Список задач из List пуст.\n");
            else
            {
                botClient.SendMessage(update.Message.Chat, $"\nСписок задач из List:");
                int taskCounter = 0;
                foreach (ToDoItem task in tasks)
                {
                    if (task.State != ToDoItemState.Active)
                        continue;

                    taskCounter++;
                    Console.WriteLine($"{taskCounter}) {task.Name} - {task.CreatedAt} - {task.Id}");
                }
            }
        }

        //////метод удаления задачи из List
        public void RemoveTaskList(List<ToDoItem> tasks, string removeTask, ITelegramBotClient botClient, Update update)
        {
            if (tasks.Count == 0)
            {
                botClient.SendMessage(update.Message.Chat, $"Ваш список задач пуст, удалять нечего.\n");

                return;
            }
            ShowTasks(tasks, botClient, update);

            bool success = int.TryParse(removeTask, out int taskNumber);

            if (success && (taskNumber > 0 && taskNumber <= tasks.Count))
            {
                botClient.SendMessage(update.Message.Chat, $"Задача {tasks[taskNumber - 1].Name} удалена.\n");
                tasks.RemoveAt(taskNumber - 1);
                ShowTasks(tasks, botClient, update);
            }
            else
            {
                botClient.SendMessage(update.Message.Chat, $"Введён некорректный номер задачи.\n");
                return;
                //RemoveTaskList(tasks, removeTask);
            }
        }


        ////работа с Array
        ////метод добавления задачи в Array Init.maxTaskLenght
        public void AddTaskArray(ToDoUser user, string taskText, ITelegramBotClient botClient, Update update)
        {
            ToDoItem[] arrayTasks = new ToDoItem[Init.tasksArray.Length + 1];

            //проверяю длину  и выбрасываю исключение если больше лимита
            if (Init.tasksArray.Length >= Init.maxTaskAmount)
                throw new TaskCountLimitException(Init.maxTaskAmount);

            //валидация строки c проверкой длины введёной задачи и выброс необходимого исключения - ДОБАВИЛ ПЕРЕГУЗКУ МЕТОДА ValidateString
            string newTask = Validate.ValidateString(taskText, Init.maxTaskLenght);

            int index = arrayTasks.Length - 1;

            //проверяю дубликаты введённой задачи
            CheckDuplicateArr(Init.tasksArray, newTask);

            //инициализирую новый объект задачи
            arrayTasks[index] = new ToDoItem
            {
                Name = newTask,
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                User = user,
                StateChangedAt = DateTime.Now,
            };

            for (int i = 0; i < index; i++)
                arrayTasks[i] = Init.tasksArray[i];

            Init.tasksArray = arrayTasks;

            botClient.SendMessage(update.Message.Chat, $"Задача \"{newTask}\" добавлена в Array задач.\n");
        }

        //////метод рендера задач из Array
        public void ShowTasks(ref ToDoItem[] tasks, ITelegramBotClient botClient, Update update)
        {
            if (tasks.Length == 0)
                botClient.SendMessage(update.Message.Chat, $"Список задач из Array пуст.\n");
            else
            {
                botClient.SendMessage(update.Message.Chat, $"\nСписок задач из Array:");
                for (int i = 0; i < tasks.Length; i++)
                    Console.WriteLine($"{i + 1}) {tasks[i].Name} - {tasks[i].CreatedAt} - {tasks[i].Id}");
            }
        }

        //////метод удаления задачи из Array
        public void RemoveTaskArray(ref ToDoItem[] tasks, string removeTask, ITelegramBotClient botClient, Update update)
        {
            if (tasks.Length == 0)
            {
                botClient.SendMessage(update.Message.Chat, $"Ваш список задач пуст, удалять нечего.\n");
                return;
            }

            ShowTasks(ref tasks, botClient, update);

            bool success = int.TryParse(removeTask, out int taskNumber);

            if (success && (taskNumber > 0 && taskNumber <= tasks.Length))
            {
                botClient.SendMessage(update.Message.Chat, $"Задача {tasks[taskNumber - 1].Name} удалена.\n");

                ToDoItem[] newTasks = new ToDoItem[tasks.Length - 1];

                int index = taskNumber;

                for (int i = 0; i < index - 1; i++)
                    newTasks[i] = tasks[i];

                for (int i = index - 1; i < newTasks.Length; i++)
                    newTasks[i] = tasks[i + 1];

                tasks = newTasks;

                ShowTasks(ref tasks, botClient, update);
            }
            else
            {
                botClient.SendMessage(update.Message.Chat, $"Введён некорректный номер задачи.\n");
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
        public (string, string) CheckAddAndRemove(string input)
        {
            string cutInput = "";
            if (input.StartsWith("/addtask") || input.StartsWith("/removetask"))
            {
                if (input.StartsWith("/addtask "))
                {
                    cutInput = input.Substring(9);
                    input = "/addtask";
                }
                
                else if (input.StartsWith("/removetask "))
                {
                    cutInput = input.Substring(12);
                    input = "/removetask";
                }
                else
                {
                    input = "unregistered user command";
                }
            }
            return (input, cutInput);
        }


        //валидация начальных значений задач
        public int CheckMaxAmount(int maxAmount, ITelegramBotClient botClient, Update update)
        {
            //присваиваю значения длин
            if (maxAmount == 0)
                maxAmount = maxAmount.GetStartValues("Введите максимально допустимое количество задач", botClient, update);

            return maxAmount;
        }

        public int CheckMaxLength(int maxLength, ITelegramBotClient botClient, Update update)
        {
            //присваиваю значения длин
            if (maxLength == 0)
                maxLength = maxLength.GetStartValues("Введите максимально допустимую длину задачи", botClient, update);

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

        public ToDoItem Add(ToDoUser user, string name)
        {
           return new ToDoItem();
        }
        public void MarkCompleted(Guid id)
        {
            // реализация
        }

        public void Delete(Guid id)
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