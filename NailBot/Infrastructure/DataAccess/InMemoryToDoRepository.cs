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

        public void Add(ToDoItem item)
        {
            ToDoList.Add(item);
        }

        public void Delete(Guid id)
        {
            ToDoList.RemoveAll(x => x.Id == id);
        }
        
        public int CountActive(Guid userId)
        {
            return GetActiveByUserId(userId).Count();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return ToDoList
                .Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return ToDoList
                .Where(x => x.User.UserId == userId)
                .ToList()
                .AsReadOnly();
        }

        //Метод должен возвращать все задачи пользователя, которые удовлетворяют предикату.
        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {

            foreach (var item in ToDoList)
            {
                
            }


            return ToDoList
                .Where(x => x.User.UserId == userId)
                .Where(predicate)
                .ToList();
        }

        public bool ExistsByName(Guid userId, string name)
        {
            return ToDoList
                .Where(x => x.User.UserId == userId)
                .Any(x => x.Name.StartsWith(name)); 
        }

        public ToDoItem? Get(Guid id)
        { 
            throw new NotImplementedException();
        }
        public void Update(ToDoItem item)
        {
            throw new NotImplementedException();
        }


    }
}
