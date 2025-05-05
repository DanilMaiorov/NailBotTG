using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    internal class UserService : IUserService
    {
        private ToDoUser currentUser;
        public ToDoUser CurrentUser
        {
            get { return currentUser; }
            set { currentUser = value; }
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName) 
        {
            CurrentUser = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now
            };

            return CurrentUser;
        }

        public ToDoUser? GetUser(long telegramUserId) 
        {
            if (CurrentUser == null)
                return null;

            return currentUser;
        }
    }
}
