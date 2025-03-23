using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    //класс для хранения данных
    internal class StartValues
    {
        //количество задач при запуске программы
        public int MaxTaskAmount { get; set; }

        //длина задачи при запуске программы
        public int MaxTaskLenght { get; set; }

        public StartValues(int maxTaskAmount, int maxTaskLenght)
        {
            MaxTaskAmount = maxTaskAmount;
            MaxTaskLenght = maxTaskLenght;
        }
    }
}
