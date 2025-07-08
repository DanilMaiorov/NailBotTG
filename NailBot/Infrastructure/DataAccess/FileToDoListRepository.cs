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
        //имя папки с ToDoList
        private readonly string _toDoListFolderName;
        //путь до текущей директории
        private readonly string _currentDirectory;

        //объявлю semaphore
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public FileToDoListRepository(string toDoListFolderName)
        {
            _toDoListFolderName = toDoListFolderName;
            _currentDirectory = GetCurrentPath();
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            await _semaphore.WaitAsync();

            try
            {
                var json = JsonSerializer.Serialize(list);
                var currentDirectory = Path.Combine(_currentDirectory, list.User.UserId.ToString());

                if (!Directory.Exists(currentDirectory))
                    Directory.CreateDirectory(currentDirectory);


                var fullPath = Path.Combine(currentDirectory, $"{list.Name}.json");
                await File.WriteAllTextAsync(fullPath, json, ct);
            }
            finally
            {
                _semaphore.Release();
            }
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
                var currentUserDirectory = Path.Combine(_currentDirectory, userId.ToString());

                if (!Directory.Exists(currentUserDirectory))
                    return toDoList.AsReadOnly(); // Возвращаем пустой список
                
                var files = Directory.EnumerateFiles(currentUserDirectory, "*.json");

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
        private string GetCurrentPath()
        {
            var directory = Directory.GetCurrentDirectory();

            var currentPath = Path.Combine(directory, _toDoListFolderName);

            if (!Directory.Exists(currentPath))
                Directory.CreateDirectory(currentPath);

            return currentPath;
        }
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


