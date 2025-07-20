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
            var parts = input.Split('|');
            return new ToDoListCallbackDto
            {
                Action = parts[0],
                ToDoListId = parts.Length > 1 && Guid.TryParse(parts[1], out var id) ? id : null
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}";
        }
    }
}
