using LinqToDB.Mapping;

namespace NailBot.Core.Entities
{
    [Table("ToDoItem")]
    public class ToDoItem
    {
        [Column("Guid"), PrimaryKey]
        public Guid Id { get; set; }

        [Column("UserId"), NotNull]
        public Guid UserId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUser.UserId))]
        public ToDoUser User { get; set; }

        [Column("Name"), NotNull]
        public string Name { get; set; }

        [Column("CreatedAt"), NotNull]
        public DateTime CreatedAt { get; set; }

        [Column("State"), NotNull]
        public ToDoItemState State { get; set; }

        [Column("StateChangedAt"), NotNull]
        public DateTime StateChangedAt { get; set; }

        [Column("Deadline"), NotNull]
        public DateTime Deadline { get; set; }

        [Column("ToDoListId"), NotNull]
        public Guid ToDoListId { get; set; }

        [Association(ThisKey = nameof(ToDoListId), OtherKey = nameof(ToDoList.Id))]
        public ToDoList? List { get; set; }
    }
}