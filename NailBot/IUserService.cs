using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NailBot
{
    public interface IUserService
    {
        User RegisterUser(long telegramUserId, string telegramUserName);
        User? GetUser(long telegramUserId);
    }
    
}
