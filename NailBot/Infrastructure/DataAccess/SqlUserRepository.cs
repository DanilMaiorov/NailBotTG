using NailBot.Core.DataAccess;
using NailBot.Core.Entities;

namespace NailBot.Infrastructure.DataAccess
{
    internal class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;
        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }
        public Task Add(ToDoUser user, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
