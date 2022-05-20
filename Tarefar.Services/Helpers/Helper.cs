using System.Text.Json;

namespace Tarefar.Services
{
    public static class Helper
    {
        public static T Clone<T>(this T obj)
        {
            string json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
