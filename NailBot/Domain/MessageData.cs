using NailBot.Core.Entities;
using Telegram.Bot.Types;

namespace NailBot.Domain;

public class MessageData
{
    public MessageData(Chat chat, string userInput, int messageId, long telegramUserId) 
    { 
        Chat = chat;
        UserInput = userInput;
        MessageId = messageId;
        TelegramUserId = telegramUserId;
    }

    public Chat Chat { get; }
    public string UserInput { get; set; }
    public int MessageId { get; }
    public long TelegramUserId { get; }
}