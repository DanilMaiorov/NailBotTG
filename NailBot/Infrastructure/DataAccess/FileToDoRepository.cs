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

        public FileToDoRepository(string toDoItemFolderName)
        {
            _toDoItemFolderName = toDoItemFolderName;
            _currentDirectory = GetCurrentDirectory2();
        }

        private string GetCurrentDirectory2()
        {
            var directory = Directory.GetCurrentDirectory();

            var currentDirectory = GetPath(directory, _toDoItemFolderName);

            if (!Directory.Exists(currentDirectory))
            {
                Directory.CreateDirectory(currentDirectory);
            }

            return currentDirectory;
        }



        //вспомогательные методы

        //получение пути


        //переписать - убрать async
        //убрать лишние!!!

        private string GetPath(string currentDirectory, string directoryName)
        {
            return Path.Combine(currentDirectory, directoryName);
        }

        //метод для возврата List в методы где возвращается IReadOnlyList<ToDoItem>
        private async Task<List<ToDoItem>> GetToDoList(Guid userId, CancellationToken ct)
        {
            var toDoList = new List<ToDoItem>();

            if (Directory.Exists(_currentDirectory))
            {
                var currentUserDirectory = GetPath(_currentDirectory, userId.ToString());

                if (Directory.Exists(currentUserDirectory))
                {
                    var files = Directory.EnumerateFiles(currentUserDirectory, "*.json");

                    foreach (var file in files)
                    {
                        try
                        {
                            var jsonContent = await File.ReadAllTextAsync(file, ct);

                            var toDoItemFromFiles = JsonSerializer.Deserialize<ToDoItem>(jsonContent);

                            if (toDoItemFromFiles != null)
                                toDoList.Add(toDoItemFromFiles);
                        }
                        catch (JsonException ex)
                        {
                            Console.WriteLine($"Ошибка десериализации файла {file}: {ex.Message}");
                        }
                        catch (IOException ex)
                        {
                            Console.WriteLine($"Ошибка чтения файла {file}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    return toDoList;
                }
            }
            else
            {
                throw new DirectoryNotFoundException($"Директория не найдена: {_currentDirectory}");
            }
            return toDoList;
        }

        //метод получения директории тудушек текущего юзера
        private string GetUserFolderPath(Guid userId, CancellationToken ct)
        {
            string currentUserDirectory = GetPath(_currentDirectory, userId.ToString());

            if (!Directory.Exists(currentUserDirectory))
                Directory.CreateDirectory(currentUserDirectory);
            
            return currentUserDirectory;
        }

        //TODO со3дать метод GetFileIndex - создать или получить
        //TODO создать метод обновления GetFileIndex
        //TODO создать метод удаления GetFileIndex

        //проверка корневой директории тудушек
        private string CheckCurrentDirectory()
        {
            var currentDirectory = GetPath(_currentDirectory, _toDoItemFolderName);

            if (!Directory.Exists(currentDirectory))
                throw new DirectoryNotFoundException($"Директория не найдена: {currentDirectory}");

            return currentDirectory;
        }


        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            var currentDirectory = GetUserFolderPath(item.User.UserId, ct);


            //TODO вызвать метод GetFileIndex - создать или получить


            
            //обновить GetFileIndex

            var json = JsonSerializer.Serialize(item);

            var fullPath = Path.Combine(currentDirectory, $"{item.Id}.json");

            await File.WriteAllTextAsync(fullPath, json);
        }

        //Индекс для оптимизации удаления ToDoItem
        //Добавить в FileToDoRepository файл индекс в json формате, в котором хранятся связки ToDoItemId и UserId
        //Наполнять индекс в методе FileToDoRepository.Add
        //Использовать и обновлять индекс в методе FileToDoRepository.Delete
        //Если файла индекса нет, то создать файл и наполнить его актуальными данными через сканирование всех папок

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
            var toDoList = await GetToDoList(userId, ct);

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
            var toDoList = await GetToDoList(userId, ct);

            if (userId == Guid.Empty)
                return toDoList.AsReadOnly();

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
            var deleteItem = await Get(id, ct);

            if (deleteItem != null)
            {
                var currentUserDirectory = GetPath(_currentDirectory, deleteItem.User.UserId.ToString());

                var filePath = Path.Combine(currentUserDirectory, id + ".json");

                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (IOException ex)
                    {
                        throw new IOException($"Ошибка при удалении: {ex.Message}");
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Файл не найден: {filePath}");
                }
            }
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
            foreach (var directory in Directory.EnumerateDirectories(_currentDirectory))
            {
                foreach (var item in Directory.EnumerateFiles(directory))
                {
                    try
                    {
                        var jsonContent = await File.ReadAllTextAsync(item, ct);
                        var toDoItemFromFiles = JsonSerializer.Deserialize<ToDoItem>(jsonContent);

                        if (toDoItemFromFiles?.Id == id)
                            return toDoItemFromFiles;
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Ошибка десериализации файла {item}: {ex.Message}");
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine($"Ошибка чтения файла {item}: {ex.Message}");
                    }
                }
            }
            return null;
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
            var toDoList = await GetToDoList(userId, ct);

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
            var toDoList = await GetToDoList(userId, ct);

            if (string.IsNullOrWhiteSpace(name))
                return false;
            
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
            var updateItem = await Get(item.Id, ct);

            if (updateItem != null)
            {
                var currentUserDirectory = GetPath(_currentDirectory, item.User.UserId.ToString());

                var filePath = GetPath(currentUserDirectory, item.Id + ".json");

                if (File.Exists(filePath))
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(item);

                        await File.WriteAllTextAsync(filePath, json);
                    }
                    catch (IOException ex)
                    {
                        throw new IOException($"Ошибка при изменении: {ex.Message}");
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Файл не найден: {filePath}");
                }
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
