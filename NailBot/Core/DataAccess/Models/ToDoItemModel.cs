using LinqToDB.Mapping;

namespace NailBot.Core.DataAccess.Models
{
    [Table("ToDoItem")]
    public class ToDoItemModel
    {
        [Column("Id"), PrimaryKey]
        public Guid Id { get; set; }

        [Column("UserId"), NotNull]
        public Guid UserId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; } = null!;

        [Column("Name"), NotNull]
        public string Name { get; set; } = string.Empty;

        [Column("CreatedAt"), NotNull]
        public DateTime CreatedAt { get; set; }

        [Column("State"), NotNull]
        public ToDoItemState State { get; set; }

        [Column("StateChangedAt")]
        public DateTime? StateChangedAt { get; set; }

        [Column("Deadline"), NotNull]
        public DateTime Deadline { get; set; }

        [Column("ToDoListId"), NotNull]
        public Guid? ToDoListId { get; set; }

        [Association(ThisKey = nameof(ToDoListId), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoListModel? List { get; set; }
    }
}