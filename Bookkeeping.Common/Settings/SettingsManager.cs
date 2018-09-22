using Bookkeeping.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace Bookkeeping.Common.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
        private readonly Dictionary<Type, Func<string, object>> _converters = new Dictionary<Type, Func<string, object>>();

        public SettingsManager()
        {
            AddConverter(x => decimal.Parse(x, CultureInfo.InvariantCulture))
                .AddConverter(x => float.Parse(x, CultureInfo.InvariantCulture))
                .AddConverter(x => double.Parse(x, CultureInfo.InvariantCulture))
                .AddConverter(x => TimeSpan.Parse(x, CultureInfo.InvariantCulture));
        }

        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            lock (_cache)
            {

                if (_cache.TryGetValue(key, out object cacheValue))
                {
                    return (T)System.Convert.ChangeType(cacheValue, typeof(T));
                }

                T value = ConfigurationManager.AppSettings.AllKeys.Contains(key) ? Convert<T>(ConfigurationManager.AppSettings[key]) : defaultValue;
                _cache.Add(key, value);
                return value;
            }
        }

        public T GetValue<T>(string key)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                return Convert<T>(ConfigurationManager.AppSettings[key]);
            }
            throw new Exception("Setting key not found: " + key);
        }

        public object GetValue(string key, object defaultValue = null)
        {
            return GetValue<object>(key, defaultValue);
        }

        private SettingsManager AddConverter<T>(Func<string, T> action)
        {
            _converters.Add(typeof(T), x => action(x));
            return this;
        }

        private T Convert<T>(string x)
        {
            Func<string, object> action;
            _converters.TryGetValue(typeof(T), out action);
            return action != null ? (T)action(x) : (T)System.Convert.ChangeType(x, typeof(T));
        }
    }
}
