using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using Otus.ToDoList.ConsoleBot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoRepository _toDoRepository;
        public ToDoReportService() { }

        public ToDoReportService(IToDoRepository toDoRepository) 
        {
            _toDoRepository = toDoRepository;
        }

        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            var tasks = _toDoRepository.GetAllByUserId(userId);
            int total = tasks.Count();
            int active = _toDoRepository.CountActive(userId);
            int completed = total - active;

            return (total, completed, active, DateTime.Now);
        }
    }
}
