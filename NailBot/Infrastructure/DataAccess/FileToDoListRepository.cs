using NailBot.Core.Entities;
using NailBot.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.Infrastructure.DataAccess
{
    public class FileToDoListRepository : IToDoListRepository
    {
        public Task Add(ToDoList list, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}

//public async Task Add(ToDoUser user, CancellationToken ct)
//{
//    var json = JsonSerializer.Serialize(user);

//    var currentDirectory = Path.Combine(_currentDirectory, _userFolderName);

//    var fullPath = Path.Combine(currentDirectory, $"{user.UserId}.json");

//    await File.WriteAllTextAsync(fullPath, json, ct);
//}
//public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
//{
//    var user = Path.Combine(_currentDirectory, userId.ToString());

//    var userList = await GetUserList(ct);

//    return userList.Find(x => x.UserId == userId);
//}

//public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
//{
//    var userList = await GetUserList(ct);

//    return userList.Find(x => x.TelegramUserId == telegramUserId);
//}

////вспомогательные методы
//private string GetCurrentPath()
//{
//    var directory = Directory.GetCurrentDirectory();

//    var currentPath = Path.Combine(directory, _userFolderName);

//    if (!Directory.Exists(currentPath))
//        Directory.CreateDirectory(currentPath);

//    return currentPath;
//}
////метод для возврата List в методы где возвращается IReadOnlyList<ToDoItem>
//private async Task<List<ToDoUser>> GetUserList(CancellationToken ct)
//{
//    var userList = new List<ToDoUser>();

//    if (Directory.Exists(_currentDirectory))
//    {
//        var files = Directory.EnumerateFiles(_currentDirectory, "*.json");

//        foreach (var file in files)
//        {
//            string jsonContent = await File.ReadAllTextAsync(file, ct);
//            var userFromFiles = JsonSerializer.Deserialize<ToDoUser>(jsonContent);

//            userList.Add(userFromFiles);
//        }
//    }
//    else
//    {
//        throw new DirectoryNotFoundException($"Директория не найдена: {_currentDirectory}");
//    }

//    return userList;
//}
