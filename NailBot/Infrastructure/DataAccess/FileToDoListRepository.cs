using NailBot.Core.Entities;
using NailBot.Core.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NailBot.Infrastructure.DataAccess
{
    public class FileToDoListRepository : IToDoListRepository
    {

        //имя папки с ToDoItem
        private readonly string _toDoItemFolderName;
        //путь до текущей директории
        private readonly string _currentDirectory;

        //объявлю semaphore
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public FileToDoListRepository(string toDoItemFolderName)
        {
            _toDoItemFolderName = toDoItemFolderName;
            _currentDirectory = Directory.GetCurrentDirectory();
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            await _semaphore.WaitAsync();

            try
            {
                var toDoItemFolderDirectoryPath = Path.Combine(_currentDirectory, _toDoItemFolderName);

                var currentUserDirectoryPath = GetUserFolderPath(list.User.UserId, toDoItemFolderDirectoryPath, ct);

                var json = JsonSerializer.Serialize(list);
                var currentDirectory = Path.Combine(currentUserDirectoryPath, list.Id.ToString());

                if (!Directory.Exists(currentDirectory))
                    Directory.CreateDirectory(currentDirectory);



                var toDoListsFolderDirectoryPath = Path.Combine(_currentDirectory, "ToDoLists", list.User.UserId.ToString());

                if (!Directory.Exists(toDoListsFolderDirectoryPath))
                    Directory.CreateDirectory(toDoListsFolderDirectoryPath);

                var fullPath = Path.Combine(toDoListsFolderDirectoryPath, $"{list.Name}.json");

                await File.WriteAllTextAsync(fullPath, json, ct);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private string GetUserFolderPath(Guid userId, string toDoFolder, CancellationToken ct)
        {
            string currentUserDirectoryPath = Path.Combine(toDoFolder, userId.ToString());

            if (!Directory.Exists(currentUserDirectoryPath))
                Directory.CreateDirectory(currentUserDirectoryPath);

            return currentUserDirectoryPath;
        }

        public Task Delete(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var list = await GetByUserId(userId, ct);

            return list?.Any(x => 
                string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)) 
                ?? false;
        }

        public Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var toDoList = new List<ToDoList>();

            await _semaphore.WaitAsync();

            try
            {
                var currentUserListsDirectory = Path.Combine(_currentDirectory, "ToDoLists", userId.ToString());

                if (!Directory.Exists(currentUserListsDirectory))
                    return toDoList.AsReadOnly(); // Возвращаем пустой список
                
                var files = Directory.EnumerateFiles(currentUserListsDirectory, "*.json");

                foreach (var file in files)
                {
                    string jsonContent = await File.ReadAllTextAsync(file, ct);
                    var toDoListFromFiles = JsonSerializer.Deserialize<ToDoList>(jsonContent);

                    toDoList.Add(toDoListFromFiles);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return toDoList.AsReadOnly();
        }

        //вспомогательные методы
    }
}


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


