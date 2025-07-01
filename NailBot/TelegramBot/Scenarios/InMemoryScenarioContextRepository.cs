namespace NailBot.TelegramBot.Scenarios
{
    public class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        //словарь в качестве хранилища
        public Dictionary<long, ScenarioContext> _scenarioContext = [];

        public Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task ResetContext(long userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
