using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot.TelegramBot.Dto
{
    public class CallbackDto
    {
        public string? Action {  get; set; }

        public static CallbackDto FromString(string input)
        {
            //ТУТ НАДО ДОПИСАТЬ ПРОВЕРКУНА СОДЕРЖАНИЕ СТРОКИ
            CallbackDto callbackDto;
            if (!input.Contains("|"))
                callbackDto = new CallbackDto { Action = input };
            else
            {
                callbackDto = new CallbackDto { Action = input };
            }
            return callbackDto;
        }
        //На вход принимает строку вида "{action}|{prop1}|{prop2}...".
        //Нужно создать CallbackDto с Action = action.
        //Нужно учесть что в строке может не быть |, тогда всю строку сохраняем в Action.



        public override string ToString() {
            return $"{Action} действие";
        }
    }
}
