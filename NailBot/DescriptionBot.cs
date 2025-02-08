using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    internal class DescriptionBot
    {
        public static void PrintDescription()
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
                $"\t\t7) Просмотр записи\n" +
                $"\t\t8) Просмотр свободных дней для записи (в ближайший и следующий календарный месяцы)\n");

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
                $"\t\t10) Редактирование расписания мастера (в ближайший и следующий календарный месяцы)\n" +
                $"\t\t11) Выбор рабочего дня мастера\n" +
                $"\t\t12) Удаления рабочего дня мастера\n" +
                $"\t\t13) Редактирование рабочего времени мастера в выбранном дне\n" +
                $"\t\t14) Установка цен на услуги\n" +
                $"\t\t15) Редактирование цен на услуги\n" +
                $"\t\t16) Просмотр свободных дней для записи (в ближайший и следующий календарный месяцы)\n");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{string.Format("{0,-10}", "Пункт 3.3")} Функции Владельца:\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\tто же что у администратора но добавляются:\n" +
                $"\t\t17) Добавить администратора\n" +
                $"\t\t18) Удалить администратора\n");
            //Ещё можно прикрутить счётчик, который будет считать выручку, но это в далёком впереди
            //Также можно прикрутить несколько студий и делать выбор между ними в начале, но это в далёком впереди

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
                $"\t\t7) Редактирование расписания (в ближайший и следующий календарный месяцы)\n" +
                $"\t\t8) Удаления рабочего дня мастера\n" +
                $"\t\t9) Выбор рабочего дня\n" +
                $"\t\t10) Редактирование рабочего времени мастера в выбранном дне\n" +
                $"\t\t11) Просмотр свободных дней для записи (в ближайший и следующий календарный месяцы)\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("4. Напишите список из максимум 4 самых базовых функций.\n");
            Console.ForegroundColor = originalTextColor;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{string.Format("{0,-10}", "Пункт 4.1")} Функции Клиента:\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\t1) Записаться на маникюр (в ближайшие 30 дней) - команда /start \n" +
                $"\t\t3) Выбрать дату записи на маникюр - команда /date\n" +
                $"\t\t3) Выбрать время записи на маникюр - команда /time\n" +
                $"\t\t4) Отмена записи на маникюр - команда /cancel\n");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"{string.Format("{0,-10}", "Пункт 4.2")} Функции Администратора:\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\t1) Записать клиента на маникюр (без имени и телефона (для начала))\n" +
                $"\t\t2) Выбрать дату записи клиента на маникюр\n" +
                $"\t\t3) Выбрать время записи клиента на маникюр\n" +
                $"\t\t4) Отмена записи клиента на маникюр\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("5. Оформите запросы к боту в формате запрос - ответ.\n");
            Console.ForegroundColor = originalTextColor;

            Console.WriteLine(
                $"\t\t1) Пользователь отправляет: \"/start\"\n\n" +
                $"\t\t2) Бот пишет: \"Здравствуйте! Выберите желаемую дату записи - показывается календарь\n\n" +
                $"\t\t3) Пользователь выбирает дату: отправляется команда выбора даты \"/date\"\n\n" +
                //Лучше сделать подтверждение даты или выбор другой даты, если был миссклик, но это + команда
                $"\t\t4) Бот пишет: \"Выбранная дата 10 февраля\n" +
                $"\t\t5) Бот пишет: \"Выберите желаемое время записи - показываются свободные временные окна для записи\n\n" +
                $"\t\t6) Пользователь выбирает время: отправляется команда выбора времени \"/time\"\n\n" +
                //Лучше сделать подтверждение времени или выбор другой даты, если был миссклик, но это + команда
                $"\t\t7) Бот пишет: \"Выбранное время 10:00\n" +
                $"\t\t8) Бот пишет: \"Потрясающе! Вы записаны на маникюр 10 февраля в 10:00. Студия находится по адресу ул. Ленина 1, оф. 1. Прекрасного дня(опционально - вечера)!\n\n" +
                $"\t\t9) Пользователь отправляет: \"Отменить запись \"/cancel\"\n\n" +
                $"\t\t10) Бот пишет: \"Ваша запись на 10 февраля в 10:00 отменена! Будем ждать вас снова!\n");
        
        }
    }
}
