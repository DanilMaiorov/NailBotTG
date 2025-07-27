using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.TelegramBot.Dto
{
    public class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }

        public static new PagedListCallbackDto FromString(string input)
        {
            var parts = input.Split('|');

            var baseDto = ToDoListCallbackDto.FromString(input);

            return new PagedListCallbackDto
            {
                Action = baseDto.Action,
                ToDoListId = baseDto.ToDoListId,

                //Page = 0

                Page = parts.Length > 2 && int.TryParse(parts[2], out var page) ? page : 0
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{Page}";
        }
    }
}
