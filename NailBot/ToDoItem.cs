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
        public Guid Id { get; init; }
        public ToDoUser User { get; init; }
        public string Name { get; init; }
        public DateTime CreatedAt { get; init; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
    }
}