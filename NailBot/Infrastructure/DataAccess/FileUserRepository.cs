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

        public FileUserRepository(string userFolderName)
        {
            _userFolderName = userFolderName;
            _currentDirectory = GetCurrentPath();
        }

        //вспомогательные методы

        private string GetCurrentPath()
        {
            var directory = Directory.GetCurrentDirectory();

            var currentPath = Path.Combine(directory, _userFolderName);

            if (!Directory.Exists(currentPath))
                Directory.CreateDirectory(currentPath);
            
            return currentPath;
        }


        //метод для возврата List в методы где возвращается IReadOnlyList<ToDoItem>
        private async Task<List<ToDoUser>> GetUserList(CancellationToken ct)
        {
            var userList = new List<ToDoUser>();

            if (Directory.Exists(_currentDirectory))
            {
                var files = Directory.EnumerateFiles(_currentDirectory, "*.json");

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
                throw new DirectoryNotFoundException($"Директория не найдена: {_currentDirectory}");
            }

            return userList;
        }

        public async Task Add(ToDoUser user, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(user);

            var currentDirectory = Path.Combine(_currentDirectory, _userFolderName);

            var fullPath = Path.Combine(currentDirectory, $"{user.UserId}.json");

            await File.WriteAllTextAsync(fullPath, json, ct);
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


}
