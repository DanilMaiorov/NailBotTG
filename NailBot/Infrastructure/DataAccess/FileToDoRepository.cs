using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using NailBot.TelegramBot;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace NailBot.Infrastructure.DataAccess
{
    internal class FileToDoRepository : IToDoRepository
    {
        //имя папки с ToDoItem
        private string _toDoItemFolderName;

        //задам локер
        private readonly object _indexLock = new();

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

        private readonly string _indexPath;

        private readonly string _indexFileName = "FileIndex.json";

        public FileToDoRepository(string toDoItemFolderName)
        {
            _toDoItemFolderName = toDoItemFolderName;
            _currentDirectory = GetCurrentPath();

            _indexPath = Path.Combine(_currentDirectory, _indexFileName);

            EnsureIndexExists();
        }

        private string GetCurrentPath()
        {
            var directory = Directory.GetCurrentDirectory();

            var currentPath = Path.Combine(directory, _toDoItemFolderName);

            if (!Directory.Exists(currentPath))
                Directory.CreateDirectory(currentPath);
            
            return currentPath;
        }

        //вспомогательные методы


        //метод для возврата List в методы где возвращается IReadOnlyList<ToDoItem>
        private async Task<List<ToDoItem>> GetToDoList(Guid userId, CancellationToken ct)
        {
            var toDoList = new List<ToDoItem>();

            if (Directory.Exists(_currentDirectory))
            {
                var currentUserDirectory = Path.Combine(_currentDirectory, userId.ToString());

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
            string currentUserDirectoryPath = Path.Combine(_currentDirectory, userId.ToString());

            if (!Directory.Exists(currentUserDirectoryPath))
                Directory.CreateDirectory(currentUserDirectoryPath);
            
            return currentUserDirectoryPath;
        }

        private void EnsureIndexExists()
        {
            lock (_indexLock)
            {
                if (!File.Exists(_indexPath))
                    RebuildIndex();
            }
        }

        //метод для наполнения индекса 
        private void RebuildIndex()
        {
            //создам словарь для хранения пары задача-пользак
            var index = new Dictionary<Guid, Guid>();

            //переберу все директории и файлы в них и занесу в словарь
            foreach (var userDir in Directory.EnumerateDirectories(_currentDirectory))
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

            SaveIndex(index);
        }

        private void SaveIndex(Dictionary<Guid, Guid> index)
        {
            lock (_indexLock)
            {
                string tempPath = null;
                try
                {
                    var directory = Path.GetDirectoryName(_indexPath);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    

                    // создам временный файл индекс
                    tempPath = Path.Combine(directory, $"{Guid.NewGuid()}.tmp");

                    var json = JsonSerializer.Serialize(index, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                    File.WriteAllText(tempPath, json);

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
                    throw new Exception($"Ошибка сохранения индекса: {ex.Message}");
                }
            }
        }

        private Dictionary<Guid, Guid> LoadIndex()
        {
            lock (_indexLock)
            {
                //проверка и перестроение индекса если он исчез по время работы
                if (!File.Exists(_indexPath))
                    RebuildIndex();
                
                try
                {
                    var json = File.ReadAllText(_indexPath);
                    return JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(json)
                        ?? new Dictionary<Guid, Guid>();
                }
                catch (Exception ex) when (ex is JsonException or IOException)
                {
                    // если файл поврежден или недоступен - перестраиваю
                    RebuildIndex();
                    var json = File.ReadAllText(_indexPath);
                    return JsonSerializer.Deserialize<Dictionary<Guid, Guid>>(json)
                        ?? new Dictionary<Guid, Guid>();
                }
            }
        }

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            var currentUserDirectoryPath = GetUserFolderPath(item.User.UserId, ct);

            var json = JsonSerializer.Serialize(item);

            var filePath = Path.Combine(currentUserDirectoryPath, $"{item.Id}.json");

            await File.WriteAllTextAsync(filePath, json, ct);

            //залочу поток и обновляю индекс
            lock (_indexLock)
            {
                try
                {
                    var index = LoadIndex();
                    index[item.Id] = item.User.UserId;
                    SaveIndex(index);
                }
                catch (Exception ex)
                {
                    // логирую ошибку и перестраиваю индекс
                    Console.WriteLine($"Ошибка обновления индекса: {ex.Message}");
                    RebuildIndex();
                }
            }
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
                var currentUserDirectoryPath = Path.Combine(_currentDirectory, deleteItem.User.UserId.ToString());

                var filePath = Path.Combine(currentUserDirectoryPath, id + ".json");

                if (File.Exists(filePath))
                {
                    //удаляю задачу
                    try
                    {
                        File.Delete(filePath);
                        //обновляю индекс
                        RebuildIndex();
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
            var index = LoadIndex();

            if (index.TryGetValue(id, out Guid userId))
            {
                var userFolderPath = Path.Combine(_currentDirectory, userId.ToString());

                if(Directory.Exists(userFolderPath))
                {
                    var toDoItemPath = Path.Combine(userFolderPath, id.ToString() + ".json");

                    var json = await File.ReadAllTextAsync(toDoItemPath, ct);

                    return JsonSerializer.Deserialize<ToDoItem>(json);
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
            if (item != null)
            {
                var currentUserDirectoryPath = Path.Combine(_currentDirectory, item.User.UserId.ToString());

                var filePath = Path.Combine(currentUserDirectoryPath, item.Id + ".json");

                if (File.Exists(filePath))
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(item);

                        await File.WriteAllTextAsync(filePath, json, ct);
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
