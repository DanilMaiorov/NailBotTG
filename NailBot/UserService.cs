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
            if (telegramUserId != 0)
                //сделал поле toDoUser статическим в UpdateHandler
                return UpdateHandler.toDoUser;
            else
                return new ToDoUser();
        }
    }
}
