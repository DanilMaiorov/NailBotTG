using NailBot.Core.DataAccess;
using NailBot.Core.Entities;

namespace NailBot.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        //хранилище пользаков
        public List<ToDoUser> UsersList = new List<ToDoUser>();

        public void Add(ToDoUser user)
        {
            Console.WriteLine($"{user.TelegramUserName} добавлен в UsersList");
            UsersList.Add(user);
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            var user = UsersList.FirstOrDefault(x => x.TelegramUserId == telegramUserId);

            if (user != null)
                return user;

            return null;
        }

        public ToDoUser? GetUser(Guid userId) => UsersList.FirstOrDefault(x => x.UserId == userId);
    }
}
