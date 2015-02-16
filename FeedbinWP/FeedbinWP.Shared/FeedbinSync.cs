using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace FeedbinWP
{
    class FeedbinSync
    {
        static private String feedbinApiUrl = "https://api.feedbin.com/v2/";
        static private String authUrl = "authentication.json";
        static private String unreadUrl = "unread_entries.json";
        static private String unreadDeleteUrl = "unread_entries/delete.json";
        static private String starredUrl = "starred_entries.json";
        static private String starredDeleteUrl = "starred_entries/delete.json";
        static private String recentlyReadUrl = "recently_read_entries.json";
        static private String entriesByIdUrl = "entries.json?ids="; // oddzielane przecinkiem, np. ids=4078,4053,4324
        static private String entriesSinceUrl = "entries.json?since="; // since=2013-02-02T14:07:33.000000Z
        static private String singleEntryUrl = "entries/"; // np entries/5034.json
        static private String subscriptionsUrl = "subscriptions.json";
        static private String singleSubscriptionUrl = "subscriptions/"; // 3.json
        
        static public async Task<bool> Login(String username, String password) 
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));

                HttpResponseMessage response = await client.GetAsync(new Uri(feedbinApiUrl + authUrl));
                if (response.IsSuccessStatusCode)
                    return true;
                else
                    return false;
            }
        }

        public static ObservableCollection<FeedbinEntry> parseEntriesJson(String data)
        {
            JsonArray parsedEntries = JsonArray.Parse(data);
            ObservableCollection<FeedbinEntry> entries = new ObservableCollection<FeedbinEntry>();
            foreach (JsonValue obj in parsedEntries)
            {
                String[] names = { "title", "url", "author", "content", "published" };
                String[] values = new String[5];

                for (int i = 0; i < names.Length; i++)
                {
                    if (obj.GetObject().GetNamedValue(names[i]).ValueType != JsonValueType.Null)
                        values[i] = obj.GetObject().GetNamedString(names[i]);
                    else
                        values[i] = " ";

                }


                FeedbinEntry entry = new FeedbinEntry((int)obj.GetObject().GetNamedNumber("id"),
                                                      (int)obj.GetObject().GetNamedNumber("feed_id"),
                                                      values[0],
                                                      values[1],
                                                      values[2],
                                                      values[3],
                                                      DateTime.Parse(values[4]));

                Regex _htmlRegex = new Regex("<.*?>");
                entry.summary = _htmlRegex.Replace(entry.content, string.Empty).Replace("\n", "").Replace("\r", "");

                entries.Add(entry);
            }
            return entries;
        }

        public static async Task<String> getEntries(String username, String password, String ids)
        {
            String data_entries = await makeApiGetRequest(username, password, feedbinApiUrl + entriesByIdUrl + ids);
            return data_entries;
        }

        public static async Task<ObservableCollection<FeedbinEntry>> getEntriesSince(String username, String password, DateTime since)
        {
            String data = await makeApiGetRequest(username, password, feedbinApiUrl + entriesSinceUrl + since.ToString("o"));
            ObservableCollection<FeedbinEntry> entries = parseEntriesJson(data);
            return entries;
        }

        public static async Task<ObservableCollection<FeedbinEntry>> getUnreadItems(String username, String password)
        {
            String data_ids = await makeApiGetRequest(username, password, feedbinApiUrl + unreadUrl);
            if (data_ids != null)
            {
                String ids = "";
                JsonArray tmp = JsonArray.Parse(data_ids);
                for (uint i = 0; i < tmp.Count; i++)
                {
                    ids += tmp.GetNumberAt(i);
                    if (i < tmp.Count - 1)
                        ids += ",";
                }

                String entries_json = await getEntries(username, password, ids);
                ObservableCollection<FeedbinEntry> entries = parseEntriesJson(entries_json);
                return entries;
            }
            else
                return null;

        }

        public static async Task<ObservableCollection<FeedbinEntry>> getStarredItems(String username, String password)
        {
            String data_ids = await makeApiGetRequest(username, password, feedbinApiUrl + starredUrl);
            if (data_ids != null)
            {
                String ids = "";
                JsonArray tmp = JsonArray.Parse(data_ids);
                for (uint i = 0; i < tmp.Count; i++)
                {
                    ids += tmp.GetNumberAt(i);
                    if (i < tmp.Count - 1)
                        ids += ",";
                }

                String entries_json = await getEntries(username, password, ids);
                ObservableCollection<FeedbinEntry> entries = parseEntriesJson(entries_json);
                return entries;
            }
            else
                return null;

        }

        public static async Task<ObservableCollection<FeedbinEntry>> getRecentlyRead(String username, String password)
        {
            String data_ids = await makeApiGetRequest(username, password, feedbinApiUrl + recentlyReadUrl);
            if (data_ids != null)
            {
                String ids = "";
                JsonArray tmp = JsonArray.Parse(data_ids);
                for (uint i = 0; i < tmp.Count; i++)
                {
                    ids += tmp.GetNumberAt(i);
                    if (i < tmp.Count - 1)
                        ids += ",";
                }

                String entries_json = await getEntries(username, password, ids);
                ObservableCollection<FeedbinEntry> entries = parseEntriesJson(entries_json);
                return entries;
            }
            else
                return null;

        }

        static public async Task<bool> addSubscription(String username, String password, String url)
        {
            StringContent json = new StringContent("{\"feed_url\": \"" + url + "\"}");
            String data = await makeApiPostRequest(username, password, feedbinApiUrl + subscriptionsUrl, json);
            if (data != null)
                return true;
            return false;
        }

        static public async Task<bool> updateSubscription(String username, String password, int feed_id, String title)
        {
            StringContent message = new StringContent("{\"title\": \"" + title + "\"}");
            String data = await makeApiPostRequest(username, password, feedbinApiUrl + singleSubscriptionUrl + "/" + feed_id + "/update.json", message);
            if (data != null)
                return true;
            return false;
        }

        static public async Task<bool> deleteSubscription(String username, String password, int feed_id)
        {
            bool result = await makeApiDeleteRequest(username, password, feedbinApiUrl + singleSubscriptionUrl + feed_id + ".json");
            return result;
        }

        static public async Task<bool> markAsRead(String username, String password, String entries)
        {
            StringContent message = new StringContent("{\"unread_entries\": [" + entries + "]}");
            String data = await makeApiPostRequest(username, password, feedbinApiUrl + unreadDeleteUrl, message);

            StringContent msg = new StringContent("{\"recently_read_entries\": [" + entries + "]}");
            String data_recentlyRead = await makeApiPostRequest(username, password, feedbinApiUrl + recentlyReadUrl, msg);

            if (data != null)
                return true;
            return false;
        }

        static public async Task<bool> markAsUnread(String username, String password, String entries)
        {
            StringContent message = new StringContent("{\"unread_entries\": [" + entries + "]}");
            String data = await makeApiPostRequest(username, password, feedbinApiUrl + unreadUrl, message);
            if (data != null)
                return true;
            return false;
        }

        static public async Task<bool> addStar(String username, String password, String entries)
        {
            StringContent message = new StringContent("{\"starred_entries\": [" + entries + "]}");
            String data = await makeApiPostRequest(username, password, feedbinApiUrl + starredUrl, message);
            if (data != null)
                return true;
            return false;
        }

        static public async Task<bool> removeStar(String username, String password, String entries)
        {
            StringContent message = new StringContent("{\"starred_entries\": [" + entries + "]}");
            String data = await makeApiPostRequest(username, password, feedbinApiUrl + starredDeleteUrl, message);
            if (data != null)
                return true;
            return false;
        }

        static private async Task<bool> makeApiDeleteRequest(String username, String password, String url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));

                var response = await client.DeleteAsync(new Uri(url));

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                    return false;
            }
        }
        
        static private async Task<String> makeApiPostRequest(String username, String password, String url, StringContent message)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));

                
                message.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await client.PostAsync(new Uri(url), message);
                var reply = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return reply;
                }
                else
                    return null;
            }
        }

        static private async Task<String> makeApiGetRequest(String username, String password, String url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));

                HttpResponseMessage response = await client.GetAsync(new Uri(url));
                
                if (response.IsSuccessStatusCode)
                {
                    String data = await response.Content.ReadAsStringAsync();
                    return data;
                }
                else
                    return null;
            }
        }


    }
}
