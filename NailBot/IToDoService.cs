using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    public interface IToDoService
    {
        //количество задач при запуске программы
        public int MaxTaskAmount { get; set; }

        //длина задачи при запуске программы
        public int MaxTaskLenght { get; set; }
        
        //список задач
        List<ToDoItem> TasksList { get; set; }





        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
        //Возвращает ToDoItem для UserId со статусом Active
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        ToDoItem Add(ToDoUser user, string name);
        void MarkCompleted(Guid id);
        void Delete(Guid id);
    }
}
