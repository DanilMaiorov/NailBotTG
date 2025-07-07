using Polly;
using System.Collections.Concurrent;

namespace NailBot.TelegramBot.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        //словарь в качестве хранилища
        public ConcurrentDictionary<long, ScenarioContext> _scenarioContext = [];

        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            return Task.FromResult(
                _scenarioContext.TryGetValue(userId, out var context)
                    ? context
                    : null
            );
        }

        public Task ResetContext(long userId, CancellationToken ct)
        {
            ScenarioContext? removedContext;

            if (_scenarioContext.TryRemove(userId, out removedContext))
                Console.WriteLine($"Контекст пользователя {userId} успешно удален.");
            else
                Console.WriteLine($"Контекст для пользователя {userId} не найден или уже был удален.");
            
            return Task.CompletedTask;
        }

        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _scenarioContext[userId] = context;
            return Task.CompletedTask;
        }
    }
}
