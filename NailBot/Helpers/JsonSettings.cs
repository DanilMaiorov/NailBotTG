using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NailBot.Helpers
{
    public static class JsonSettings 
    {
        public static JsonSerializerOptions SerializerOptions()
        {
            return new JsonSerializerOptions { WriteIndented = true };
        }
        
    }
}
