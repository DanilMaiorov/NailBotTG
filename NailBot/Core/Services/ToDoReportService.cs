using NailBot.Core.DataAccess;
using NailBot.Core.Entities;

namespace NailBot.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _toDoRepository;
        public ToDoReportService(IToDoRepository toDoRepository) 
        {
            _toDoRepository = toDoRepository;
        }

        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId)
        {
            var tasks = await _toDoRepository.GetAllByUserId(userId);
            int total = tasks.Count;
            int active = tasks.Count(x => x.User.UserId == userId && x.State == ToDoItemState.Active);
            int completed = total - active;

            return (total, completed, active, DateTime.Now);
        }
    }
}