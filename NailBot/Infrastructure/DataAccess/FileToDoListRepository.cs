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
        //путь до директории с ToDoList
        private readonly string _toDoListDirectory;

        private readonly string _indexPath;

        private readonly string _indexFileName = "FileIndexLists.json";

        //объявлю semaphore
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);


        public Task Initialization { get; }

        public FileToDoListRepository(string toDoListFolderName, string toDoItemFolderName)
        {
            _toDoListFolderName = toDoListFolderName;
            _toDoItemFolderName = toDoItemFolderName;
            _currentDirectory = Directory.GetCurrentDirectory();

            _toDoListDirectory = Helper.GetDirectoryPath(_currentDirectory, _toDoListFolderName);

            _indexPath = Path.Combine(_currentDirectory, _toDoListFolderName, _indexFileName);

            Initialization = EnsureIndexExists();           
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            var userId = list.User.UserId.ToString();

            try
            {
                //получение пути до папки с тудушками пользака
                var currentUserToDoItemDirectoryPath = Helper.GetDirectoryPath(_currentDirectory, _toDoItemFolderName, userId);

                var json = JsonSerializer.Serialize(list);

                //проверка папки-списка тудушек пользка
                Helper.CheckOrCreateDirectory(currentUserToDoItemDirectoryPath, list.Id.ToString());

                //получение пути до папки с листами пользака
                var currentUserToDoListsFolderDirectoryPath = Helper.GetDirectoryPath(_toDoListDirectory, userId);

                //получение пути до json файла списка 
                var fullPath = Path.Combine(currentUserToDoListsFolderDirectoryPath, $"{list.Id}.json");

                //создание json файла списка 
                await File.WriteAllTextAsync(fullPath, json, ct);

                //работа с файл индексом
                try
                {
                    var index = await LoadIndex();
                    index[list.Id] = list.User.UserId;
                    await SaveIndex(index);
                }
                catch (Exception ex)
                {
                    // логирую ошибку и перестраиваю индекс
                    Console.WriteLine($"Ошибка обновления индекса: {ex.Message}");
                    RebuildIndex();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }


        public async Task Delete(Guid id, CancellationToken ct)
        {
            var deleteItem = await Get(id, ct);
            var userId = deleteItem.User.UserId.ToString();
            var listId = deleteItem.Id.ToString();

            if (deleteItem != null)
            {
                var currentUserListsDirectoryPath = Path.Combine(_currentDirectory, _toDoListFolderName, userId);

                var filePath = Path.Combine(currentUserListsDirectoryPath, listId + ".json");

                if (File.Exists(filePath))
                {
                    //удаляю задачу
                    File.Delete(filePath);
                    //обновляю индекс
                    RebuildIndex();
                }
                else
                {
                    throw new FileNotFoundException($"Файл не найден: {filePath}");
                }
            }
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

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct);
            try
            {
                var index = await LoadIndex();

                if (index.TryGetValue(id, out Guid userId))
                {
                    var userListsFolderPath = Path.Combine(_toDoListDirectory, userId.ToString());

                    if (Directory.Exists(userListsFolderPath))
                    {
                        var toDoListPath = Path.Combine(userListsFolderPath, id.ToString() + ".json");

                        var json = await File.ReadAllTextAsync(toDoListPath, ct);

                        return JsonSerializer.Deserialize<ToDoList>(json);
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return null;
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var toDoList = new List<ToDoList>();

            await _semaphore.WaitAsync(ct);

            try
            {
                var currentUserListsDirectory = Helper.GetDirectoryPath(_toDoListDirectory, userId.ToString());

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

        //методы работы с файл индексом
        private async Task EnsureIndexExists()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!File.Exists(_indexPath))
                    await RebuildIndex();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        //метод для наполнения индекса 
        private async Task RebuildIndex()
        {
            //создам словарь для хранения пары задача-пользак
            var index = new Dictionary<Guid, Guid>();

            //переберу все директории и файлы в них и занесу в словарь
            foreach (var userDir in Directory.EnumerateDirectories(_toDoListDirectory))
            {
                var userId = Path.GetFileName(userDir);

                if (Guid.TryParse(userId, out var userGuid))
                {
                    foreach (var filePath in Directory.EnumerateFiles(userDir))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

                        if (Guid.TryParse(fileName, out var todoId))
                        {
                            if (File.Exists(filePath))
                                index[todoId] = userGuid;
                        }
                    }
                }
            }
            await SaveIndex(index);
        }

        private async Task SaveIndex(Dictionary<Guid, Guid> index)
        {
            string tempPath = null;
            try
            {
                var directory = Path.GetDirectoryName(_indexPath);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // создам временный файл индекс
                tempPath = Path.Combine(directory, $"{Guid.NewGuid()}.tmp");

                var json = JsonSerializer.Serialize(index, JsonSettings.SerializerOptions());

                await File.WriteAllTextAsync(tempPath, json);

                if (!File.Exists(tempPath))
                    throw new IOException($"Не удалось создать временный файл: {tempPath}");


                // атомарная запись через временный файл
                if (File.Exists(_indexPath))
                    // заменяю существующий файл
                    File.Replace(tempPath, _indexPath, null);
                else
                    // если файл не существует - переименовываю временный файл
                    File.Move(tempPath, _indexPath);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения индекса: {ex.Message}");
            }
        }

        private async Task<Dictionary<Guid, Guid>> LoadIndex()
        {
            //проверка и перестроение индекса если он исчез по время работы
            if (!File.Exists(_indexPath))
                RebuildIndex();

            try
            {
                var json = await File.ReadAllTextAsync(_indexPath);
                return JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(json)
                    ?? new Dictionary<Guid, Guid>();
            }
            catch (Exception ex) when (ex is JsonException or IOException)
            {
                // если файл поврежден или недоступен - перестраиваю
                RebuildIndex();
                var json = await File.ReadAllTextAsync(_indexPath);
                return JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(json)
                    ?? new Dictionary<Guid, Guid>();
            }
        }

    }
}


