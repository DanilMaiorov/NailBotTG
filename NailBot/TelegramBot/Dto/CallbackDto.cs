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
            var parts = input.Split('|', 2);
            return new CallbackDto
            {
                Action = parts[0]
            };
        }

        public override string ToString() {
            return Action;
        }
    }
}
