using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NailBot.Infrastructure.DataAccess
{
    internal class FileUserRepository : IUserRepository
    {
        //имя папки с ToDoItem
        private string _userFolderName;

        public string UserFolderName
        {
            get { return _userFolderName; }
            set { _userFolderName = value; }
        }

        //путь до текущей директории
        private string _currentDirectory;

        public string СurrentDirectory
        {
            get { return _currentDirectory; }
            set { _currentDirectory = value; }
        }

        public FileUserRepository(string userFolderName, string currentDirectory)
        {
            _userFolderName = userFolderName;
            _currentDirectory = currentDirectory;
        }

        //вспомогательные методы

        //получение пути
        private async Task<string> GetPath(string currentDirectory, string directoryName)
        {
            return Path.Combine(currentDirectory, directoryName);
        }

        //метод для возврата List в методы где возвращается IReadOnlyList<ToDoItem>
        private async Task<List<ToDoUser>> GetUserList(CancellationToken ct)
        {
            var currentDirectory = await GetPath(_currentDirectory, _userFolderName);

            var userList = new List<ToDoUser>();

            if (Directory.Exists(currentDirectory))
            {
                var files = Directory.EnumerateFiles(currentDirectory, "*.json");

                foreach (var file in files)
                {
                    try
                    {
                        string jsonContent = await File.ReadAllTextAsync(file, ct);
                        var userFromFiles = JsonSerializer.Deserialize<ToDoUser>(jsonContent);

                        userList.Add(userFromFiles);
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
                throw new DirectoryNotFoundException($"Директория не найдена: {currentDirectory}");
            }
            return userList;
        }

        public async Task Add(ToDoUser user, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(user);

            var currentDirectory = await GetPath(_currentDirectory, _userFolderName);

            var fullPath = Path.Combine(currentDirectory, $"{user.UserId}.json");

            File.WriteAllText(fullPath, json);
        }

        #region старая реализация public async Task Add(ToDoUser user, CancellationToken ct)
        //public async Task Add(ToDoUser user, CancellationToken ct)
        //{
        //    UsersList.Add(user);
        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);
        //}
        #endregion

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            var userList = await GetUserList(ct);

            return userList?.Find(x => x.UserId == userId);
        }

        #region старая реализация public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        //public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        //{
        //    var user = UsersList.FirstOrDefault(x => x.UserId == userId);

        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);

        //    return user;
        //}
        #endregion


        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            var userList = await GetUserList(ct);

            return userList?.Find(x => x.TelegramUserId == telegramUserId);
        }

        #region старая реализация public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        //public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        //{
        //    var user = UsersList.FirstOrDefault(x => x.TelegramUserId == telegramUserId);

        //    //сделаю искусственную задержку для асинхронности
        //    await Task.Delay(1, ct);

        //    return user;
        //}
        #endregion
    }

    //Оптимизация поиска ToDoItem по UserId
    //Реализовать в FileToDoRepository хранение ToDoItem в отдельных json файлах, сгуппированных по UserId в папках
    //Имя папки: "{ToDoItem.User.UserId}"
    //Имя файла: "{ToDoItem.Id}.json"

    //Индекс для оптимизации удаления ToDoItem
    //Добавить в FileToDoRepository файл индекс в json формате, в котором хранятся связки ToDoItemId и UserId
    //Наполнять индекс в методе FileToDoRepository.Add
    //Использовать и обновлять индекс в методе FileToDoRepository.Delete
    //Если файла индекса нет, то создать файл и наполнить его актуальными данными через сканирование всех папок

    //Ознакомительное примечание (выполнять не нужно):
    //Для безопасной работы с файлами в многопоточной среде рекомендуется использовать синхронизацию потоков, чтобы в один
    //момент времени с файлом работал только один поток.Это поможет избежать race conditions, повреждение данных, IO исключений и тд.
    //Для этого хорошо подходит SemaphoreSlim, так как он поддерживает асинхронность, оптимизирован для внутрипроцессной
    //синхронизации и не использует объекты ядра ОС. Например, в нашем случае можно было использовать отдельные SemaphoreSlim
    //для каждого UserId.Данная тема выходит за рамки курса.
}
