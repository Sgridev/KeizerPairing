using System.Text.Json;

namespace KeizerPairing.Shared
{
    public static class CloneExtensions
    {
        public static T Clone<T>(this T cloneable) where T : new()
        {
            var toJson = JsonSerializer.Serialize(cloneable);
            return JsonSerializer.Deserialize<T>(toJson);
        }
    }
}
