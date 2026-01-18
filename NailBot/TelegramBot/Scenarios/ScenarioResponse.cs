using NailBot.Core.Enums;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace NailBot.TelegramBot.Scenarios;

public class ScenarioResponse
{
    public ScenarioResponse(ScenarioResult result, Chat chat) 
    {
        Result = result;
        Chat = chat;
    }
    public ScenarioResult Result { get; set; }
    public Chat Chat { get; set; }
    public string Message { get; set; }
    public bool IsEdit { get; set; } = false;
    public bool HasText { get; set; } = true;
    public ReplyMarkup Keyboard { get; set; }
    public List<(string message, ReplyMarkup keyboard)> Messages { get; set; }
    public List<(int messageId, string message, InlineKeyboardMarkup keyboard)> EditMessages { get; set; }
}