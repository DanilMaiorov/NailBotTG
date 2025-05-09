using NailBot.Core.DataAccess;
using NailBot.Core.Entities;

namespace NailBot.Core.Services
{
    internal class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository) 
        {
            _userRepository = userRepository;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var newUser = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now
            };

            //добавлю юзера в репозиторий
            _userRepository.Add(newUser);

            return newUser;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            var user = _userRepository.GetUserByTelegramUserId(telegramUserId);

            if (user != null)
            {
                return _userRepository.GetUser(user.UserId);
            }

            return null;
        }
    }
}
