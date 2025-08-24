using NailBot.Core.Enums;

namespace NailBot.TelegramBot.Scenarios
{
    public class ScenarioContext
    {
        public Guid UserId { get; }
        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; }
        public Dictionary<string, object> Data { get; set; } = [];
        public ScenarioContext(ScenarioType scenario, Guid userId)
        {
            CurrentScenario = scenario;
            UserId = userId;
        }
    }
}
