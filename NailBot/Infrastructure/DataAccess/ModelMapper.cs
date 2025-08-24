using NailBot.Core.DataAccess.Models;
using NailBot.Core.Entities;

namespace NailBot.Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            return new ToDoUser
            {
                UserId = model.UserId,
                TelegramUserId = model.TelegramUserId,
                TelegramUserName = model.TelegramUserName,
                RegisteredAt = model.RegisteredAt,
            };
        }
        public static ToDoUserModel MapToModel(ToDoUser entity)
        {
            return new ToDoUserModel
            {
                UserId = entity.UserId,
                TelegramUserId = entity.TelegramUserId,
                TelegramUserName = entity.TelegramUserName,
                RegisteredAt = entity.RegisteredAt,
            };
        }
        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            return new ToDoItem
            {
                Id = model.Id,
                User = model.User != null ? MapFromModel(model.User) : null,
                Name = model.Name,
                CreatedAt = model.CreatedAt,
                State = model.State,
                StateChangedAt = model.StateChangedAt,
                Deadline = model.Deadline,
                List = model.List != null ? MapFromModel(model.List) : null
            };
        }
        public static ToDoItemModel MapToModel(ToDoItem entity)
        {
            return new ToDoItemModel
            {
                Id = entity.Id,
                UserId = entity.User.UserId,
                User = null!,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                State = entity.State,
                StateChangedAt = entity.StateChangedAt,
                Deadline = entity.Deadline,
                ToDoListId = entity.List?.Id,
                List = null
            };
        }
        public static ToDoList MapFromModel(ToDoListModel model)
        {
            return new ToDoList
            {
                Id = model.Id,
                Name = model.Name,
                User = model.User != null ? MapFromModel(model.User) : null,
                CreatedAt = model.CreatedAt,
            };
        }
        public static ToDoListModel MapToModel(ToDoList entity)
        {
            return new ToDoListModel
            {
                Id = entity.Id,
                Name = entity.Name,
                UserId = entity.User.UserId,
                User = null!,
                CreatedAt = entity.CreatedAt,
            };
        }
    }
}



