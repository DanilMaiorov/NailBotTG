using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    internal class UserService : IUserService
    {
        public User RegisterUser(long telegramUserId, string telegramUserName) 
        {
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = new DateTime()
            };

            return newUser; 

        }
        public User? GetUser(long telegramUserId) 
        { 

            return new User(); 

        }

        //User IUserService.RegisterUser(long telegramUserId, string telegramUserName)
        //{
        //    throw new NotImplementedException();
        //}

        //User? IUserService.GetUser(long telegramUserId)
        //{
        //    throw new NotImplementedException();
        //}
    }
}

//Заполнять telegramUserId и telegramUserName нужно из значений Update.Message.From


//Добавить использование IUserService в UpdateHandler. Получать IUserService нужно через конструктор


//При команде /start нужно вызвать метод IUserService.RegisterUser.
//Если пользователь не зарегистрирован, то ему доступны только команды /help /info
