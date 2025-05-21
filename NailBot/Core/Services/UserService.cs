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

        public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            var newUser = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now
            };

            await _userRepository.Add(newUser, ct);

            return newUser;
        }

        public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken ct)
        {
            var user = await _userRepository.GetUserByTelegramUserId(telegramUserId, ct);

            return user?.UserId != null 
                ? await _userRepository.GetUser(user.UserId, ct)
                : null;
        }
    }
}
