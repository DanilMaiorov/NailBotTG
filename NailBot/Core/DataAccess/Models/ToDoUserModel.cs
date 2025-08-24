using LinqToDB.Mapping;

namespace NailBot.Core.DataAccess.Models
{
    [Table("ToDoUser")]
    public class ToDoUserModel
    {
        [Column("UserId"), PrimaryKey]
        public Guid UserId { get; set; }

        [Column("TelegramUserId"), NotNull]
        public long TelegramUserId { get; set; }

        [Column("TelegramUserName"), NotNull]
        public string TelegramUserName { get; set; }

        [Column("RegisteredAt"), NotNull]
        public DateTime RegisteredAt { get; set; }
    }
}
