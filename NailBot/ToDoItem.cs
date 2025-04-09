using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    public enum ToDoItemState { Active, Completed };
    public class ToDoItem
    {
        public Guid Id { get; init; }
        public ToDoUser User { get; init; }
        public string Name { get; init; }
        public DateTime CreatedAt { get; init; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
    }
}


//Добавление класса ToDoItem


//Добавить класс ToDoItem
//Свойства
//Guid Id
//User User
//string Name
//DateTime CreatedAt
//ToDoItemState State
//DateTime? StateChangedAt - обновляется при изменении State
//Добавить использование класса ToDoItem вместо хранения только имени задачи