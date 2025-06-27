using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Cody.Core.Logging;

namespace Cody.Core.Common
{
    public static partial class Configuration
    {
        private static readonly Dictionary<string, object> _config = new Dictionary<string, object>();

        private static ILog _logger;

        public static void Initialize(ILog logger)
        {
            _logger = logger;
        }

        public static void AddFromJsonFile(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    var configuration = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    AddFromDictionary(configuration);

                    _logger.Debug("Configuration loaded.");
                    _logger.Debug(json);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed loading configuration from: '{path}'", ex);
                }
            }
        }

        public static void AddFromEnviromentVariableJsonFile(string variable)
        {
            var path = Environment.GetEnvironmentVariable(variable);
            AddFromJsonFile(path);
        }

        private static void AddFromDictionary(Dictionary<string, object> dictionary)
        {
            if (dictionary == null) return;

            foreach (var pair in dictionary)
            {
                if (_config.ContainsKey(pair.Key)) _config[pair.Key] = pair.Value;
                else _config.Add(pair.Key, pair.Value);
            }
        }

        private static T Get<T>(T defaultValue, [CallerMemberName] string key = null)
        {
            try
            {
                return (T)(_config.TryGetValue(key, out object value) ? value : defaultValue);
            }
            catch (Exception ex)
            {
                _logger.Error($"Cannot get value for key:'{key}'", ex);
            }

            return defaultValue;
        }


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
