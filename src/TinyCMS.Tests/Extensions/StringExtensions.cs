using Newtonsoft.Json;

namespace TinyCMS.Tests.Extensions
{

    public static class StringExtensions
    {
        public static T ToJson<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
