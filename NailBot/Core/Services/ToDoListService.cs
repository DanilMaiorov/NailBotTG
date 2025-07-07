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
        private readonly IToDoListRepository _toDoListRepository;

        public ToDoListService(IToDoListRepository toDoListRepository)
        {
            _toDoListRepository = toDoListRepository;
        }

        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            // Размер имени списка не может быть больше 10 символов
            if (string.IsNullOrWhiteSpace(name) || name.Length > 10)
                throw new ArgumentException("Название списка не может быть пустым и не должно превышать 10 символов.");

            //Название списка должно быть уникально в рамках одного ToDoUser

            var toDoList = await _toDoListRepository.GetByUserId(user.UserId, ct);
            //ПРОДОЛЖИТЬ ТУТ РЕАЛИЗОВАЛ МЕТОД GetByUserId

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





    }
}
