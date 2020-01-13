using System.Text;
using Newtonsoft.Json;

namespace HC.TFPlugin
{
    public class StateHelper
    {
        public static byte[] Serialize<T>(T state) where T : class, new()
        {
            return state == null
                ? null
                : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(state));
        }

        public static T Deserialize<T>(byte[] state) where T : class, new()
        {
            return state == null
                ? null
                : JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(state));
        }
    }
}