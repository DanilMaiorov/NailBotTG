using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace NailBot.Infrastructure.DataAccess
{
    internal class FileToDoRepository : IToDoRepository
    {
        //имя папки с ToDoItem
        private string _toDoItemFolderName;

        public string ToDoItemFolderName
        {
            get { return _toDoItemFolderName; }
            set { _toDoItemFolderName = value; }
        }

        //путь до текущей директории
        private string _currentDirectory;

        public string СurrentDirectory
        {
            get { return _currentDirectory; }
            set { _currentDirectory = value; }
        }

        public FileToDoRepository(string toDoItemFolderName, string currentDirectory)
        {
            _toDoItemFolderName = toDoItemFolderName;
            _currentDirectory = currentDirectory;
        }

        //, "..", "..", ".."


        //вспомогательные методы
        
        //получение пути
        private async Task<string> GetPath(string directoryName)
        {
            return Path.Combine(_currentDirectory, directoryName);
        }

        //метод для возврата List в методы где возвращается IReadOnlyList<ToDoItem>
        private async Task<List<ToDoItem>> GetToDoList(CancellationToken ct)
        {
            var path = await GetPath(_toDoItemFolderName);

            var toDoList = new List<ToDoItem>();

            if (Directory.Exists(path))
            {
                var files = Directory.EnumerateFiles(path, "*.json");       

                foreach (var file in files)
                {
                    try
                    {
                        string jsonContent = await File.ReadAllTextAsync(file, ct);

                        var toDoItemFromFiles = JsonSerializer.Deserialize<ToDoItem>(jsonContent);

                        toDoList.Add(toDoItemFromFiles);
                    }
                    catch (JsonException ex)
                    {
                        throw new JsonException($"Ошибка десериализации файла {file}: {ex.Message}");
                    }
                    catch (IOException ex)
                    {
                        throw new IOException($"Ошибка чтения файла {file}: {ex.Message}");
                    }
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"Директория не найдена: {path}");
            }
            return toDoList;
        }

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(item);

            var rootPath = await GetPath(_toDoItemFolderName);

            var fullPath = Path.Combine(rootPath, $"{item.Id}.json");

            File.WriteAllText(fullPath, json);
        }

        #region старая реализация public async Task Add(ToDoItem item, CancellationToken ct)
        //public async Task Add(ToDoItem item, CancellationToken ct)
        //{
        //    ToDoList.Add(item);
        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);
        //}
        #endregion

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            var toDoList = await GetToDoList(ct);

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1, ct);

            return toDoList
                .Where(x => x.User.UserId == userId)
                .ToList()
                .AsReadOnly();
        }

        #region старая реализация public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        //public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        //{
        //    var result = ToDoList
        //    .Where(x => x.User.UserId == userId)
        //    .ToList()
        //    .AsReadOnly();

        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);

        //    return result;
        //}
        #endregion


        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            var toDoList = await GetToDoList(ct);

            if (userId == Guid.Empty)
            {
                return toDoList.AsReadOnly();
            }

            //сделаю искусственную задержку для асинхронности
            await Task.Delay(1, ct);
            return toDoList
                .Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active)
                .ToList()
                .AsReadOnly();
        }

        #region старая реализация public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        //public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        //{
        //    if (userId == Guid.Empty)
        //    {
        //        return ToDoList.AsReadOnly();
        //    }

        //    var result = ToDoList
        //        .Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active)
        //        .ToList()
        //        .AsReadOnly();

        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);

        //    return result;
        //}
        #endregion


        public async Task Delete(Guid id, CancellationToken ct)
        {
            var path = await GetPath(_toDoItemFolderName);

            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException($"Директория не найдена: {path}");
            }

            string filePath = Path.Combine(path, id.ToString() + ".json");

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Console.WriteLine("Файл удалён.");
                }
                catch (IOException ex)
                {
                    throw new IOException($"Ошибка при удалении: {ex.Message}");
                }
            }
            else
            {
                throw new FileNotFoundException($"Файл не найден: {path}");
            } 

            await Task.Delay(1, ct);
        }

        #region старая реализация public async Task Delete(Guid id, CancellationToken ct)
        //public async Task Delete(Guid id, CancellationToken ct)
        //{
        //    ToDoList.RemoveAll(x => x.Id == id);
        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);
        //}
        #endregion


        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            var toDoList = await GetToDoList(ct);

            return toDoList.FirstOrDefault(x => x.Id == id);
        }

        #region старая реализация public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        //public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        //{
        //    var item = ToDoList.FirstOrDefault(x => x.Id == id);

        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);

        //    return item;
        //}
        #endregion


        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            var countList = await GetActiveByUserId(userId, ct);

            return countList.Count;
        }

        #region старая реализация public async Task<int> CountActive(Guid userId, CancellationToken ct)
        //public async Task<int> CountActive(Guid userId, CancellationToken ct)
        //{
        //    var countList = await GetActiveByUserId(userId, ct);

        //    return countList.Count;
        //}
        #endregion


        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var toDoList = await GetToDoList(ct);

            return toDoList
                .Where(x => x.User.UserId == userId)
                .Where(predicate)
                .ToList(); 
        }

        #region старая реализация public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        //public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        //{
        //    var result = ToDoList
        //        .Where(x => x.User.UserId == userId)
        //        .Where(predicate)
        //        .ToList();

        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);

        //    return result;
        //}
        #endregion


        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var toDoList = await GetToDoList(ct);

            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            return toDoList
                .Where(x => x.User.UserId == userId)
                .Any(x => x.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase)); 
        }

        #region старая реализация public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        //public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        //{
        //    if (string.IsNullOrWhiteSpace(name))
        //    {
        //        return false;
        //    }
        //    var result = ToDoList
        //        .Where(x => x.User.UserId == userId)
        //        .Any(x => x.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase));

        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);

        //    return result;
        //}
        #endregion


        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            var path = await GetPath(_toDoItemFolderName);

            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Директория не найдена: {path}");
                throw new DirectoryNotFoundException($"Директория не найдена: {path}");
            }

            string filePath = Path.Combine(path, item.Id.ToString() + ".json");

            if (File.Exists(filePath))
            {
                try
                {
                    var json = JsonSerializer.Serialize(item);

                    File.WriteAllText(filePath, json);
                    Console.WriteLine("Файл изменён.");
                }
                catch (IOException ex)
                {
                    throw new IOException($"Ошибка при изменении: {ex.Message}");
                }
            }
            else
            {
                throw new FileNotFoundException($"Директория не найдена: {path}");
            }
        }

        #region старая реализация public async Task Update(ToDoItem item, CancellationToken ct)
        //public async Task Update(ToDoItem item, CancellationToken ct)
        //{
        //    var updateIndex = ToDoList.FindIndex(x => x.Id == item.Id);

        //    if (updateIndex != -1)
        //    {
        //        ToDoList[updateIndex] = item;

        //        //сделаю искусственную задержку для асинхронности
        //        await Task.Delay(1, ct);
        //    }
        //    else
        //    {
        //        throw new KeyNotFoundException($"Задча с номером {item.Id} не найдена");
        //    }
        //}
        #endregion
    }
}
