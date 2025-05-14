using NailBot.Core.DataAccess;
using NailBot.Core.Entities;

namespace NailBot.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        //хранилище пользаков
        private readonly List<ToDoUser> UsersList = [];

        public async Task Add(ToDoUser user)
        {
            UsersList.Add(user);
            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId)
        {
            var user = UsersList.FirstOrDefault(x => x.TelegramUserId == telegramUserId);

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1);

            return user;
        }

        public async Task<ToDoUser?> GetUser(Guid userId)
        {
            var user = UsersList.FirstOrDefault(x => x.UserId == userId);

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1);

            return user;
        }
    }
}
