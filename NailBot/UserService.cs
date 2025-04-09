using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    internal class UserService : IUserService
    {
        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName) 
        {
            return new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now
            };
        }

        public ToDoUser? GetUser(long telegramUserId) 
        { 
            return new ToDoUser(); 
        }
    }
}
