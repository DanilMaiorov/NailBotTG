using NailBot.Core.DataAccess;
using NailBot.Core.Entities;
using NailBot.Helpers;
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
        private readonly string _userFolderName;

        //путь до текущей директории
        private readonly string _currentDirectory;

        //путь до текущей директории
        private readonly string _toDoUserDirectory;


        // Создаем семафор: разрешаем только ОДНОМУ потоку доступ одновременно (1, 1)
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public FileUserRepository(string userFolderName)
        {
            _userFolderName = userFolderName;
            _currentDirectory = Directory.GetCurrentDirectory();

            _toDoUserDirectory = Helper.GetDirectoryPath(_currentDirectory, _userFolderName);
        }
        public async Task Add(ToDoUser user, CancellationToken ct)
        {
            await _semaphore.WaitAsync(ct); // Захватываем семафор

            try
            {
                var json = JsonSerializer.Serialize(user);

                var fullPath = Path.Combine(_toDoUserDirectory, $"{user.UserId}.json");

                await File.WriteAllTextAsync(fullPath, json, ct);
            }
            finally
            {
                _semaphore.Release(); // Освобождаем семафор, даже если произошла ошибка
            }
        }
        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken ct)
        {
            var user = Path.Combine(_currentDirectory, userId.ToString()); 

            var userList = await GetUserList(ct);

            return userList.Find(x => x.UserId == userId);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken ct)
        {
            var userList = await GetUserList(ct);

            return userList.Find(x => x.TelegramUserId == telegramUserId);
        }
    
        //вспомогательные методы
        //метод для возврата List в методы где возвращается IReadOnlyList<ToDoItem>

        private async Task<List<ToDoUser>> GetUserList(CancellationToken ct)
        {
            var userList = new List<ToDoUser>();

            await _semaphore.WaitAsync(ct); // Захватываем семафор для чтения файлов
            try
            {
                if (Directory.Exists(_toDoUserDirectory))
                {
                    var files = Directory.EnumerateFiles(_toDoUserDirectory, "*.json");

                    foreach (var file in files)
                    {
                        string jsonContent = await File.ReadAllTextAsync(file, ct);
                        var userFromFile = JsonSerializer.Deserialize<ToDoUser>(jsonContent);

                        if (userFromFile != null)
                            userList.Add(userFromFile);
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException($"Директория не найдена: {_toDoUserDirectory}");
                }
            }
            finally
            {
                _semaphore.Release(); // Освобождаем семафор
            }

            return userList;
        }    
    
    }
}
