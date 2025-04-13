using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//добавляю using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot.Types;

namespace NailBot
{
    public enum ToDoItemState { Active, Completed };
    public class ToDoItem
    {
        // это свойство User User как по заданию
        public User User1 { get; init; }


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