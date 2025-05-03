using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        public List<ToDoItem> ToDoList = new List<ToDoItem>();

        //бот для сообщений
        private ITelegramBotClient _botClient;
        public ITelegramBotClient BotClient
        {
            get { return _botClient; }
            set { _botClient = value; }
        }
        private static Chat chat;
        public Chat Chat
        {
            get { return chat; }
            set { chat = value; }
        }

        public void Add(ToDoItem item)
        {
            ToDoList.Add(item);
        }

        public int CountActive(Guid userId)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            ToDoList.RemoveAll(x => x.Id == id);
        }

        public bool ExistsByName(Guid userId, string name)
        {
            throw new NotImplementedException();
        }

        public ToDoItem? Get(Guid id)
        {
            throw new NotImplementedException();
        }



        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return ToDoList
                .Where(x => x.State == ToDoItemState.Active)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return ToDoList;
        }



        public void Update(ToDoItem item)
        {
            throw new NotImplementedException();
        }
    }
}
