using LinqToDB.Mapping;

namespace NailBot.Core.Entities
{
    [Table("ToDoUser")]
    public class ToDoUserModel
    {
        [Column("Guid"), PrimaryKey]
        public Guid UserId { get; set; }

        [Column("TelegramUserId"), NotNull]
        public long TelegramUserId { get; set; }

        [Column("TelegramUserName"), NotNull]
        public string TelegramUserName { get; set; }

        [Column("RegisteredAt"), NotNull]
        public DateTime RegisteredAt { get; set; }
    }
}
