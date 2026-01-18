using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace NailBot.Core.Services;

public class MessageService : IMessageService
{
    private readonly ITelegramBotClient _botClient;
    
    public MessageService(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task SendMessage(Chat chat, string message, CancellationToken ct)
    {
        await _botClient.SendMessage(chat, message, cancellationToken: ct);
    }

    public async Task SendMessage(Chat chat, string message, ParseMode parseMode, CancellationToken ct)
    {
        await _botClient.SendMessage(chat, message, parseMode, cancellationToken: ct);
    }

    public async Task SendMessage(Chat chat, string message, ReplyMarkup replyMarkup, CancellationToken ct)
    {
        await _botClient.SendMessage(chat, message, replyMarkup: replyMarkup, cancellationToken: ct);
    }

    public async Task SendMultiMessage(Chat chat, List<(string message, ReplyMarkup keyboard)> messages, CancellationToken ct)
    {
        foreach (var message in messages)
            await SendMessage(chat, message.message, message.keyboard, ct);
    }

    public async Task EditMessage(Chat chat, int messageId, InlineKeyboardMarkup keyboard, CancellationToken ct)
    {
        await _botClient.EditMessageReplyMarkup(chat, messageId, replyMarkup: keyboard, cancellationToken: ct);
    }

    public async Task EditMultiMessage(Chat chat, List<(int messageId, string message, InlineKeyboardMarkup keyboard)> messages, CancellationToken ct)
    {
        foreach (var message in messages)
            await EditMessage(chat, message.messageId, message.keyboard, ct);
    }
    
    public async Task EditMultiMessageWithText(Chat chat, string textMessage, List<(int messageId, string message, InlineKeyboardMarkup keyboard)> messages, CancellationToken ct)
    {
        foreach (var message in messages)
            await EditMessageText(chat, message.messageId, message.message, message.keyboard, ct);
        
        await SendMessage(chat, textMessage, ct);
    }

    public async Task EditMessageText(Chat chat, int messageId, string text, InlineKeyboardMarkup keyboard, CancellationToken ct)
    {
        await _botClient.EditMessageText(chat, messageId, text, replyMarkup: keyboard, cancellationToken: ct);
    }
}