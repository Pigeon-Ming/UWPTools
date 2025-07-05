using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage;

namespace UWPTools.Models
{
    public class SettingsManager
    {
        public static ApplicationDataContainer UserSettings;

        public static void InitUserSettings()
        {
            UserSettings = ApplicationData.Current.LocalSettings;
            //InitSettings();
        }

        public static bool SetSetting(string key, object value)
        {
            UserSettings.Values[key] = value;
            return true;
        }

        public static bool SetInitSetting(string key, object value)
        {
            if (UserSettings.Values.ContainsKey(key))
                return false;
            UserSettings.Values.Add(key, value);
            return true;
        }

        public static object GetSettingContent(string key)
        {
            object value;
            if (UserSettings.Values.TryGetValue(key, out value))
                return value;
            else
                return null;
        }

        public static string GetSettingContentAsString(string key)
        {
            object value;
            if (UserSettings.Values.TryGetValue(key, out value))
                return value.ToString();
            else
                return null;
        }
    }
}
