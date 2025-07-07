namespace NailBot.Core.Entities
{
    public class ToDoList
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ToDoUser User { get; set; }
        public DateTime CreatedAt { get; set; }

        public ToDoList()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow; // Рекомендуется использовать UtcNow для консистентности
        }
    }
}


