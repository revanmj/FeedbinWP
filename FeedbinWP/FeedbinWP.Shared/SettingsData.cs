using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;

namespace FeedbinWP
{
    class SettingsData
    {
        public bool syncStarred { get; set; }
        public bool syncRecent { get; set; }
        public bool syncRead { get; set; }
        public bool syncAtStart { get; set; }
        public bool loggedIn { get; set; }
        public int daysToSync { get; set; }
        public DateTime lastSync { get; set; }

        public void readSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            syncAtStart = (bool)localSettings.Values["syncAtStart"];
            syncStarred = (bool)localSettings.Values["syncStarred"];
            syncRecent = (bool)localSettings.Values["syncRecent"];
            syncRead = (bool)localSettings.Values["syncRead"];
            loggedIn = (bool)localSettings.Values["loggedIn"];
            daysToSync = (int)localSettings.Values["daysToSync"];
            lastSync = DateTime.Parse((String)localSettings.Values["lastSync"]);
        }

        public void saveSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["syncAtStart"] = syncAtStart;
            localSettings.Values["syncRead"] = syncRead;
            localSettings.Values["syncRecent"] = syncRecent;
            localSettings.Values["syncStarred"] = syncStarred;
            localSettings.Values["loggedIn"] = loggedIn;
            localSettings.Values["lastSync"] = lastSync.ToString();
            localSettings.Values["daysToSync"] = daysToSync;
        }

        public void setToday()
        {
            lastSync = DateTime.Now;
        }

        public void setDefaults()
        {
            syncAtStart = false;
            syncStarred = true;
            syncRecent = true;
            syncRead = false;
            loggedIn = false;
            daysToSync = -3;
            setToday();
            saveSettings();
        }
    }
}
