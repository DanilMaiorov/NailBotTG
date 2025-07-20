namespace NailBot.Core.Entities
{
    public class ToDoUser
    {
        public Guid UserId { get; init; }
        public long TelegramUserId { get; init; }
        public string TelegramUserName { get; init; }
        public DateTime RegisteredAt { get; init; }
    }
}
