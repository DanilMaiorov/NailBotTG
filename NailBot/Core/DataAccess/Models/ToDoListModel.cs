using LinqToDB.Mapping;

namespace NailBot.Core.DataAccess.Models
{
    [Table("ToDoList")]
    public class ToDoListModel
    {
        [Column("Id"), PrimaryKey]
        public Guid Id { get; set; }

        [Column("Name"), NotNull]
        public string Name { get; set; }

        [Column("UserId"), NotNull]
        public Guid UserId { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUserModel User { get; set; }

        [Column("CreatedAt"), NotNull]
        public DateTime CreatedAt { get; set; }
    }
}