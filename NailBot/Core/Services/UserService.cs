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

            //добавлю юзера в репозиторий
            _userRepository.Add(CurrentUser);

            return CurrentUser;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {


            if (_userRepository.GetUserByTelegramUserId(telegramUserId) == null)
                return null;

            CurrentUser = _userRepository.GetUser(CurrentUser.UserId);

            return currentUser;
        }

        

        
    }
}
