using LinqToDB;
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
        //РЕАЛИЗОВАНО
        public async Task Add(ToDoUser user, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            await dbContext.InsertAsync(ModelMapper.MapToModel(user), token: ct);
        }
        //РЕАЛИЗОВАНО
        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var user = await dbContext.ToDoUsers.FirstOrDefaultAsync(u => u.UserId == userId, token: ct);

            return ModelMapper.MapFromModel(user);
        }
        //РЕАЛИЗОВАНО
        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var userModel = await dbContext.ToDoUsers.FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, token: ct);

            return userModel != null ? ModelMapper.MapFromModel(userModel) : null;
        }
    }
}
