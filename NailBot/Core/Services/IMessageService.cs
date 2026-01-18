using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace NailBot.Core.Services;

public interface IMessageService
{
    Task SendMessage(Chat chat, string message, CancellationToken ct);
    Task SendMessage(Chat chat, string message, ParseMode parseMode, CancellationToken ct);
    Task SendMessage(Chat chat, string message, ReplyMarkup replyMarkup, CancellationToken ct);
    Task SendMultiMessage(Chat chat, List<(string message, ReplyMarkup keyboard)> messages, CancellationToken ct);
    Task EditMessage(Chat chat, int messageId, InlineKeyboardMarkup keyboard, CancellationToken ct);
    Task EditMessageText(Chat chat, int messageId, string text, InlineKeyboardMarkup keyboard, CancellationToken ct);
    Task EditMultiMessage(Chat chat, List<(int messageId, string message, InlineKeyboardMarkup keyboard)> messages, CancellationToken ct);
    Task EditMultiMessageWithText(Chat chat, string textMessage, List<(int messageId, string message, InlineKeyboardMarkup keyboard)> messages, CancellationToken ct);
}