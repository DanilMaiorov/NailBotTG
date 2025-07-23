namespace NailBot.TelegramBot.Dto
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public Guid? ToDoItemId { get; set; }

        public static new ToDoItemCallbackDto FromString(string input)
        {
            var parts = input.Split('|');
            return new ToDoItemCallbackDto
            {
                Action = parts[0],
                ToDoItemId = parts.Length > 1 && Guid.TryParse(parts[1], out var id) ? id : null
            };
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoItemId}";
        }

    }
}
