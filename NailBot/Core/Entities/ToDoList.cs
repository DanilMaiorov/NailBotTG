using LinqToDB.Mapping;

namespace NailBot.Core.Entities
{
    [Table("ToDoList")]
    public class ToDoList
    {
        [Column("Guid"), PrimaryKey]
        public Guid Id { get; set; }

        [Column("Name"), NotNull]
        public string Name { get; set; }

        [Column("UserId"), NotNull]
        public Guid UserId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(User.UserId))]
        public ToDoUser User { get; set; }

        [Column("CreatedAt"), NotNull]
        public DateTime CreatedAt { get; set; }
    }
}