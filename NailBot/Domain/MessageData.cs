using NailBot.Core.Entities;
using Telegram.Bot.Types;

namespace NailBot.Domain;

public class MessageData
{
    public MessageData(Chat chat, string userInput, int messageId, ToDoUser user) 
    { 
        Chat = chat;
        UserInput = userInput;
        MessageId = messageId;  
        User = user ?? null;
        TelegramUserId = User?.TelegramUserId ?? 0;
    }

    public Chat Chat { get; }
    public string UserInput { get; set; }
    public int MessageId { get; }
    public ToDoUser? User { get; }
    public long TelegramUserId { get; }
}