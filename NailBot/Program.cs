
using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NailBot

{
    internal class Program
    {
        static void Main(string[] args)
        {
            ConsoleColor originalTextColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("1. Выберите название темы\n");
            Console.ForegroundColor = originalTextColor;
            Console.WriteLine($"{"Пункт 1.",-10}\"Тема моего проекта \"NailBot\" - бот для записи на маникюр\"\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("2. Напишите список ролей для максимально возможной версии вашего приложения, то есть все ваши, даже самые смелые, пожелания.\n");
            Console.ForegroundColor = originalTextColor;
            Console.WriteLine($"{"Пункт 2.",-10}\"Всего планируется 4 роли. Пользователь, Администратор, Мастер, Владелец. На первом этапе только 2 роли - клиент и администратор(он же мастер и владелец)\"\n\n");


            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("3. Напишите какие функции будут у каждой роли.\n");
            Console.ForegroundColor = originalTextColor;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{string.Format("{0,-10}", "Пункт 3.1")} Функции Клиента:\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\t1) Выбрать услугу\n" +
                $"\t\t2) Выбрать мастера\n" +
                $"\t\t3) Выбрать дату записи на услугу\n" +
                $"\t\t4) Выбрать время записи на услугу\n" +
                $"\t\t5) Подтвердить время записи на услугу\n" +
                $"\t\t6) Отмена записи на услугу\n" +
                $"\t\t7) Просмотр записи\n");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{string.Format("{0,-10}", "Пункт 3.2")} Функции Администратора:\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\t1) Выбрать услугу для записи клиента\n" +
                $"\t\t2) Выбрать мастера для записи клиента\n" +
                $"\t\t3) Выбрать дату клиента записи на услугу\n" +
                $"\t\t4) Выбрать время клиента записи на услугу\n" +
                $"\t\t5) Подтвердить время клиента записи на услугу\n" +
                $"\t\t6) Просмотр всех записей на день/неделю/месяц\n" +
                $"\t\t7) Отмена записи клиента на услугу\n" +
                $"\t\t8) Добавить мастера\n" +
                $"\t\t9) Удалить мастера\n" +
                $"\t\t10) Редактирование расписания мастера (в ближайшие 30 дней)\n" +
                $"\t\t11) Выбор рабочего дня мастера\n" +
                $"\t\t12) Удаления рабочего дня мастера\n" +
                $"\t\t13) Редактирование рабочего времени мастера в выбранном дне\n" +
                $"\t\t14) Установка цен на услуги\n" +
                $"\t\t15) Редактирование цен на услуги\n");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{string.Format("{0,-10}", "Пункт 3.3")} Функции Владельца:\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\tто же что у администратора но добавляются:\n" +
                $"\t\t16) Добавить администратора\n" +
                $"\t\t17) Удалить администратора\n");
            //Ещё можно прикрутить счётчик, который будет считать выручку, но это в далёком впереди

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{string.Format("{0,-10}", "Пункт 3.4")} Функции Мастера:\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\t1) Выбрать услугу для записи клиента\n" +
                $"\t\t2) Выбрать дату клиента записи на услугу\n" +
                $"\t\t3) Выбрать время клиента записи на услугу\n" +
                $"\t\t4) Подтвердить время клиента записи на услугу\n" +
                $"\t\t5) Просмотр всех записей на день/неделю/месяц\n" +
                $"\t\t6) Отмена записи клиента на услугу\n" +
                $"\t\t10) Редактирование расписания (в ближайшие 30 дней)\n" +
                $"\t\t12) Удаления рабочего дня мастера\n" +
                $"\t\t13) Выбор рабочего дня\n" +
                $"\t\t14) Редактирование рабочего времени мастера в выбранном дне\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("4. Напишите список из максимум 4 самых базовых функций.\n");
            Console.ForegroundColor = originalTextColor;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{string.Format("{0,-10}", "Пункт 4.1")} Функции Клиента:\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\t1) Записаться на маникюр (в ближайшие 30 дней)\n" +
                $"\t\t3) Выбрать дату записи на маникюр\n" +
                $"\t\t3) Выбрать время записи на маникюр\n" +
                $"\t\t4) Отмена записи на маникюр\n");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{string.Format("{0,-10}", "Пункт 4.2")} Функции Администратора:\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\t1) Записать клиента на маникюр (без имени и телефона (для начала)) (в ближайшие 30 дней))\n" +
                $"\t\t2) Выбрать дату записи клиента на маникюр\n" +
                $"\t\t3) Выбрать время записи клиента на маникюр\n" +
                $"\t\t4) Отмена записи клиента на маникюр\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("5. Оформите запросы к боту в формате запрос - ответ.\n");
            Console.ForegroundColor = originalTextColor;

            //5. Оформите запросы к боту в формате запрос - ответ.Например:
            //  Пользователь отправляет "/start"
            //  Бот пишет "Введите ваше имя"
            //  Пользователь пишет "Лёша"
            //  Бот пишет "Привет, Лёша! =)"
        }
    }
}

