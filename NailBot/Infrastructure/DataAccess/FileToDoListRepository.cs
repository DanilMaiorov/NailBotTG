using NailBot.Core.Entities;
using NailBot.Core.Services;
using NailBot.Helpers;
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
        //имя папки с ToDoItem
        private readonly string _toDoItemFolderName;
        //путь до текущей директории
        private readonly string _currentDirectory;

        //объявлю semaphore
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public FileToDoListRepository(string toDoListFolderName, string toDoItemFolderName)
        {
            _toDoListFolderName = toDoListFolderName;
            _toDoItemFolderName = toDoItemFolderName;
            _currentDirectory = Directory.GetCurrentDirectory();
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);

            try
            {
                //получение пути до папки с тудушками пользака
                var currentUserToDoItemDirectoryPath = Helper.GetDirectoryPath(_currentDirectory, _toDoItemFolderName, list.User.UserId.ToString());

                var json = JsonSerializer.Serialize(list);

                //проверка папки-списка тудушек пользка
                Helper.CheckOrCreateDirectory(currentUserToDoItemDirectoryPath, list.Id.ToString());

                //получение пути до папки с листами пользака
                var currentUserToDoListsFolderDirectoryPath = Helper.GetDirectoryPath(_currentDirectory, _toDoListFolderName, list.User.UserId.ToString());

                //получение пути до json файла списка 
                var fullPath = Path.Combine(currentUserToDoListsFolderDirectoryPath, $"{list.Name}.json");

                //создание json файла списка 
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

            await _semaphore.WaitAsync(ct);

            try
            {
                var currentUserListsDirectory = Helper.GetDirectoryPath(_currentDirectory, _toDoListFolderName, userId.ToString());

                if (!Directory.Exists(currentUserListsDirectory))
                    return toDoList.AsReadOnly(); // верну пустой список
                
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
    }
}


