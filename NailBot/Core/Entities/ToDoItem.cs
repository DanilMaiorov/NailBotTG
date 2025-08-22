namespace NailBot.Core.Entities
{
    public class ToDoItem
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ToDoUser User { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public DateTime Deadline { get; set; }
        public Guid ToDoListId { get; set; }
        public ToDoList? List { get; set; }
    }
}