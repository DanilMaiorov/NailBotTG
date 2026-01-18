using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.Core.Entities
{
    public static class Constants
    {
        public const string DeadlineFormat = "dd.MM.yyyy";
        
        public const string StartBotMessage = "Для запуска бота необходимо нажать на кнопку ниже или ввести /start";
        public const string RegisterMessage = "Спасибо за регистрацию";
        public const string ErrorMessage = "Ошибка: введена некорректная команда. Пожалуйста, введите команду заново.\n";
        public const string TaskNotFoundMessage = "Задачи не найдены.\n";
        public const string CancelScenarioMessage = "Сценарий отменён. Выбирай что хочешь сделать?";
        public const string ExitBotMessage = "Нажмите CTRL+C (Ввод) для остановки бота";
        
        
        public const string CommonExceptionMessage = "Нажмите CTRL+C (Ввод) для остановки бота";
        
        public const string InfoMessage = "Это ToDoListBot версии 1.0 Beta. Бот для записи дел.\n";
        public const string CommandDescriptionMessage = $@"Это Todo List Bot - телеграм бот записи дел.
            Введя команду ""/start"" бот предложит тебе ввести имя
            Введя команду ""/help"" ты получишь справку о командах
            Введя команду ""/addtask"" будет предложено ввести название задачи и при успешном вводе, задача будет добавлена
            Введя команду ""/cancel"" ты сможешь отменить добавление новой задачи
            Введя команду ""/show"" ты сможешь увидеть список активных задач в списках
            Введя команду ""/find"" *название задачи* ты сможешь увидеть список всех задач начинающихся с названия задачи
            Введя команду ""/report"" ты получишь отчёт по задачам
            Введя команду ""/info"" ты получишь информацию о версии программы
            Введя команду ""/exit"" бот попрощается и завершит работу";
    }
}
