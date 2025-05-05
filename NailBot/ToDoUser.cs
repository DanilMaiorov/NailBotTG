using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    public class ToDoUser
    {
        public Guid UserId { get; init; }
        public long TelegramUserId { get; init; }
        public string TelegramUserName { get; init; }
        public DateTime RegisteredAt { get; init; }
    }
}
