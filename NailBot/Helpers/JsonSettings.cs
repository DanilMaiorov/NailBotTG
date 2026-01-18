using System.Text.Json;

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
