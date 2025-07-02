using Polly;

namespace NailBot.TelegramBot.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        //словарь в качестве хранилища
        public Dictionary<long, ScenarioContext> _scenarioContext = [];

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
            _scenarioContext.Remove(userId);
            return Task.CompletedTask;
        }

        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _scenarioContext[userId] = context;
            return Task.CompletedTask;
        }
    }
}
