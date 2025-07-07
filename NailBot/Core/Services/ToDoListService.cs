using NailBot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.Core.Services
{
    public class ToDoListService : IToDoListService
    {
        public Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }


        //Размер имени списка не может быть больше 10 символов

        //Название списка должно быть уникально в рамках одного ToDoUser
    }
}
