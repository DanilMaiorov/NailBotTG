namespace NailBot.Core.Entities
{
    public enum ToDoItemState { Active, Completed };
    public class ToDoItem
    {
        public Guid Id { get; init; }
        public ToDoUser User { get; init; }
        public string Name { get; init; }
        public DateTime CreatedAt { get; init; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
    }
}