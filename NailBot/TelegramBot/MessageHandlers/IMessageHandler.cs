using Telegram.Bot;
using Telegram.Bot.Types;

namespace NailBot.TelegramBot.MessageHandlers;

public interface IMessageHandler
{
    Task HandleAsync(ITelegramBotClient botClient, Update update, Message message, CancellationToken ct);
}