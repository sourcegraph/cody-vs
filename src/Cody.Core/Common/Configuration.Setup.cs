using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cody.Core.Common
{
    public static partial class Configuration
    {
        private static Dictionary<string, object> config = new Dictionary<string, object>();

        public static void AddFromJsonFile(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    config = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    AddFromDictionary(config);
                }
                catch { }
            }
        }

        public static void AddFromEnviromentVariableJsonFile(string variable)
        {
            var path = Environment.GetEnvironmentVariable(variable);
            AddFromJsonFile(path);
        }

        public static void AddFromDictionary(Dictionary<string, object> dictionary)
        {
            if (dictionary == null) return;

            foreach (var pair in dictionary)
            {
                if (config.ContainsKey(pair.Key)) config[pair.Key] = pair.Value;
                else config.Add(pair.Key, pair.Value);
            }
        }

        private static T Get<T>(T defaultValue, [CallerMemberName] string key = null) => (T)(config.TryGetValue(key, out object value) ? value : defaultValue);


        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
