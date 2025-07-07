using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.TelegramBot.Dto
{
    public class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId {  get; set; }

        public static new ToDoListCallbackDto FromString(string input)
        {
            return new ToDoListCallbackDto { Action = "action", ToDoListId = Guid.NewGuid() };
        }
        //На вход принимает строку ввида "{action}|{toDoListId}|{prop2}...".
        //Нужно создать ToDoListCallbackDto с Action = action и ToDoListId = toDoListId.
        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}";
        }
    }
}
