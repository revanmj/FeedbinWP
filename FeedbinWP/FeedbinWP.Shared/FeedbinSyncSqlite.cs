using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Linq;
using System.Collections.ObjectModel;
using Windows.Data.Json;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace FeedbinWP
{
    class FeedbinSyncSqlite
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

        public static async Task<int> getRecentlyRead(String username, String password)
        {
            String data_ids = await makeApiGetRequest(username, password, feedbinApiUrl + recentlyReadUrl);
            if (data_ids != null)
            {
                String ids = "";
                JsonArray tmp = JsonArray.Parse(data_ids);
                SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
                for (uint i = 0; i < tmp.Count; i++)
                {
                    int id = (int)tmp.GetNumberAt(i);
                    FeedbinEntry entry = await db.Table<FeedbinEntry>().Where(x => x.id == id).FirstOrDefaultAsync();
                    if (entry != null)
                    {
                        entry.recent = true;
                        await db.UpdateAsync(entry);
                    } else
                    {
                        ids += tmp.GetNumberAt(i);
                        if (i < tmp.Count - 1)
                            ids += ",";
                    }
                }
                if (ids.Length > 0)
                {
                    if (ids.LastIndexOf(",") == ids.Length - 1)
                        ids = ids.Substring(0, ids.Length - 2);
                    String entries_json = await getEntries(username, password, ids);
                    ObservableCollection<FeedbinEntry> list = parseEntriesJson(entries_json);
                    await db.InsertAllAsync(list);
                }
                return 1;
            }
            else
                return 0;

        }

        public static async Task<int> getUnreadItems(String username, String password)
        {
            String data_ids = await makeApiGetRequest(username, password, feedbinApiUrl + unreadUrl);
            if (data_ids != null)
            {
                String ids = "";
                JsonArray tmp = JsonArray.Parse(data_ids);
                SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
                for (uint i = 0; i < tmp.Count; i++)
                {
                    int id = (int)tmp.GetNumberAt(i);
                    FeedbinEntry entry = await db.Table<FeedbinEntry>().Where(x => x.id == id).FirstOrDefaultAsync();
                    if (entry != null)
                    {
                        entry.read = false;
                        await db.UpdateAsync(entry);
                    }
                    else
                    {
                        ids += tmp.GetNumberAt(i);
                        if (i < tmp.Count - 1)
                            ids += ",";
                    }
                }
                if (ids.Length > 0)
                {
                    if (ids.LastIndexOf(",") == ids.Length - 1)
                        ids = ids.Substring(0, ids.Length - 2);
                    String entries_json = await getEntries(username, password, ids);
                    ObservableCollection<FeedbinEntry> list = parseEntriesJson(entries_json);
                    await db.InsertAllAsync(list);
                }
                return 1;
            }
            else
                return 0;

        }

        public static async Task<int> getStarredItems(String username, String password)
        {
            String data_ids = await makeApiGetRequest(username, password, feedbinApiUrl + starredUrl);
            if (data_ids != null)
            {
                String ids = "";
                JsonArray tmp = JsonArray.Parse(data_ids);
                SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
                for (uint i = 0; i < tmp.Count; i++)
                {
                    int id = (int)tmp.GetNumberAt(i);
                    FeedbinEntry entry = await db.Table<FeedbinEntry>().Where(x => x.id == id).FirstOrDefaultAsync();
                    if (entry != null)
                    {
                        entry.starred = true;
                        await db.UpdateAsync(entry);
                    }
                    else
                    {
                        ids += tmp.GetNumberAt(i);
                        if (i < tmp.Count - 1)
                            ids += ",";
                    }
                }
                if (ids.Length > 0)
                {
                    if (ids.LastIndexOf(",") == ids.Length - 1)
                        ids = ids.Substring(0, ids.Length - 2);
                    String entries_json = await getEntries(username, password, ids);
                    ObservableCollection<FeedbinEntry> list = parseEntriesJson(entries_json);
                    await db.InsertAllAsync(list);
                }
                return 1;
            }
            else
                return 0;

        }

        public static async Task<int> getEntriesSince(String username, String password, DateTime since)
        {
            String data = await makeApiGetRequest(username, password, feedbinApiUrl + entriesSinceUrl + since.ToString("o"));
            ObservableCollection<FeedbinEntry> entries = parseEntriesJson(data);
            if (entries.Count > 0)
            {
                SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
                await db.InsertAllAsync(entries);
                return 1;
            }
            return 0;
        }

        public static async Task<String> getEntries(String username, String password, String ids)
        {
            String data_entries = await makeApiGetRequest(username, password, feedbinApiUrl + entriesByIdUrl + ids);
            return data_entries;
        }

        static private ObservableCollection<FeedbinEntry> parseEntriesJson(String data)
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

        static private async Task<bool> makeApiDeleteRequest(String username, String password, String url)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password)));

                var response = await client.DeleteAsync(new Uri(feedbinApiUrl + subscriptionsUrl));

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

                var response = await client.PostAsync(new Uri(feedbinApiUrl + subscriptionsUrl), message);
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
