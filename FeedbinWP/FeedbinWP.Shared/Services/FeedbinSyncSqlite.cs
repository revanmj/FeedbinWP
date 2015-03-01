using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Linq;
using Windows.Data.Json;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Windows.Security.Credentials;
using FeedbinWP.Data;

namespace FeedbinWP.Services
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

        public static async Task<bool> Login(String username, String password)
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

        public static async Task<int> getRecentlyRead()
        {
            String data_ids = await makeApiGetRequest(feedbinApiUrl + recentlyReadUrl);
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
                    String entries_json = await getEntries(ids);
                    List<FeedbinEntry> list = await parseEntriesJson(entries_json, 3);
                    await db.InsertAllAsync(list);
                }
                return 1;
            }
            else
                return 0;

        }

        public static async Task<int> getUnreadItems()
        {
            String data_ids = await makeApiGetRequest(feedbinApiUrl + unreadUrl);
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
                    String entries_json = await getEntries(ids);
                    List<FeedbinEntry> list = await parseEntriesJson(entries_json, 1);
                    await db.InsertAllAsync(list);
                }
                return 1;
            }
            else
                return 0;

        }

        public static async Task<int> getStarredItems()
        {
            String data_ids = await makeApiGetRequest(feedbinApiUrl + starredUrl);
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
                    String entries_json = await getEntries(ids);
                    List<FeedbinEntry> list = await parseEntriesJson(entries_json, 2);
                    await db.InsertAllAsync(list);
                }
                return 1;
            }
            else
                return 0;

        }

        public static async Task<int> getEntriesSince(DateTime since)
        {
            String data = await makeApiGetRequest(feedbinApiUrl + entriesSinceUrl + since.ToString("o"));
            List<FeedbinEntry> entries = await parseEntriesJson(data);
            if (entries.Count > 0)
            {
                SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
                await db.InsertAllAsync(entries);
                return 1;
            }
            return 0;
        }

        public static async Task<String> getEntries(String ids)
        {
            String data_entries = await makeApiGetRequest(feedbinApiUrl + entriesByIdUrl + ids);
            return data_entries;
        }

        public static async Task<bool> markSingleAsRead(FeedbinEntry entry)
        {
            StringContent message = new StringContent("{\"unread_entries\": [" + entry.id.ToString() + "]}");
            String data = await makeApiPostRequest(feedbinApiUrl + unreadDeleteUrl, message);

            StringContent msg = new StringContent("{\"recently_read_entries\": [" + entry.id.ToString() + "]}");
            String data_recentlyRead = await makeApiPostRequest(feedbinApiUrl + recentlyReadUrl, msg);

            entry.read = true;
            entry.recent = true;

            SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
            await db.UpdateAsync(entry);

            if (data != null)
                return true;
            return false;
        }

        public static async Task<bool> markSingleAsUnread(FeedbinEntry entry)
        {
            StringContent message = new StringContent("{\"unread_entries\": [" + entry.id.ToString() + "]}");
            String data = await makeApiPostRequest(feedbinApiUrl + unreadUrl, message);

            entry.read = false;

            SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
            await db.UpdateAsync(entry);

            if (data != null)
                return true;
            return false;
        }

        public static async Task<bool> markAllAsRead()
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
            List<FeedbinEntry> list = await db.Table<FeedbinEntry>().Where(x => x.read == false).ToListAsync();

            String ids = "";
            for (int i = 0; i < list.Count; i++)
            {
                ids += list[i].id.ToString();
                if (i < list.Count - 1)
                    ids += ",";
                list[i].read = true;
            }
            
            StringContent message = new StringContent("{\"unread_entries\": [" + ids + "]}");
            String data = await makeApiPostRequest(feedbinApiUrl + unreadDeleteUrl, message);
            
            await db.UpdateAllAsync(list);

            if (data != null)
                return true;
            return false;
        }

        public static async Task<bool> addSingleStar(FeedbinEntry entry)
        {
            StringContent message = new StringContent("{\"starred_entries\": [" + entry.id.ToString() + "]}");
            String data = await makeApiPostRequest(feedbinApiUrl + starredUrl, message);

            entry.starred = true;

            SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
            await db.UpdateAsync(entry);

            if (data != null)
                return true;
            return false;
        }

        public static async Task<bool> removeSingleStar(FeedbinEntry entry)
        {
            StringContent message = new StringContent("{\"starred_entries\": [" + entry.id.ToString() + "]}");
            String data = await makeApiPostRequest(feedbinApiUrl + starredDeleteUrl, message);

            entry.starred = false;

            SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
            await db.UpdateAsync(entry);

            if (data != null)
                return true;
            return false;
        }

        public static async Task<bool> addSubscription(String url)
        {
            StringContent json = new StringContent("{\"feed_url\": \"" + url + "\"}");
            String data = await makeApiPostRequest(feedbinApiUrl + subscriptionsUrl, json);
            if (data != null)
                return true;
            return false;
        }

        public static async Task<bool> updateSubscription(int feed_id, String title)
        {
            StringContent message = new StringContent("{\"title\": \"" + title + "\"}");
            String data = await makeApiPostRequest(feedbinApiUrl + singleSubscriptionUrl + "/" + feed_id + "/update.json", message);
            if (data != null)
                return true;
            return false;
        }

        public static async Task<bool> deleteSubscription(int feed_id)
        {
            bool result = await makeApiDeleteRequest(feedbinApiUrl + singleSubscriptionUrl + feed_id + ".json");
            return result;
        }

        public static async Task<int> getSubscriptions()
        {
            String data_entries = await makeApiGetRequest(feedbinApiUrl + subscriptionsUrl);
            if (data_entries != null)
            {
                List<FeedbinSubscription> subs = parseSubsriptionsJson(data_entries);
                SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
                await db.InsertAllAsync(subs);
                return 1;
            }
            else
                return 0;
        }

        public static async Task<FeedbinSubscription> getSingleSubscription(int id)
        {
            String data_entries = await makeApiGetRequest(feedbinApiUrl + singleSubscriptionUrl + id + ".json");
            if (data_entries != null)
            {
                List<FeedbinSubscription> subs = parseSubsriptionsJson(data_entries);
                return subs[0];
            }
            else
                return null;
        }

        public static async Task<int> cleanupDatabase(int entriesToKeep)
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");

            List<FeedbinEntry> list = await db.Table<FeedbinEntry>().OrderByDescending(x => x.published).ToListAsync();

            if (list.Count > entriesToKeep)
                for (int i = list.Count; i > entriesToKeep; i--)
                    await db.DeleteAsync(list[i]);
            return 1;
        }

        public static async Task<int> resetReadStatus()
        {
            SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
            List<FeedbinEntry> unreadEntries = await db.Table<FeedbinEntry>().Where(x => x.read == false).ToListAsync();
            for (int i = 0; i < unreadEntries.Count; i++)
            {
                unreadEntries[i].read = true;
            }
            await db.UpdateAllAsync(unreadEntries);

            return 1;
        }

        private static async Task<List<FeedbinEntry>> parseEntriesJson(String data, int mode = 0)
        {
            JsonArray parsedEntries = JsonArray.Parse(data);
            List<FeedbinEntry> entries = new List<FeedbinEntry>();
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

                SQLiteAsyncConnection db = new SQLiteAsyncConnection("feedbinData.db");
                FeedbinSubscription sub = await db.Table<FeedbinSubscription>().Where(x => x.feed_id == entry.feed_id).FirstOrDefaultAsync();
                entry.feed_name = sub.title;

                switch (mode)
                {
                    case 1:
                        entry.read = false;
                        break;
                    case 2:
                        entry.starred = true;
                        break;
                    case 3:
                        entry.recent = true;
                        break;
                }


                Regex _htmlRegex = new Regex("<.*?>");
                entry.summary = _htmlRegex.Replace(entry.content, string.Empty).Trim();

                entries.Add(entry);
            }
            return entries;
        }

        private static List<FeedbinSubscription> parseSubsriptionsJson(String data) 
        {
            JsonArray parsedEntries = JsonArray.Parse(data);
            List<FeedbinSubscription> subs = new List<FeedbinSubscription>();
            foreach (JsonValue obj in parsedEntries)
            {
                String[] names = { "title", "feed_url", "site_url" };
                String[] values = new String[3];

                for (int i = 0; i < names.Length; i++)
                {
                    if (obj.GetObject().GetNamedValue(names[i]).ValueType != JsonValueType.Null)
                        values[i] = obj.GetObject().GetNamedString(names[i]);
                    else
                        values[i] = " ";

                }


                FeedbinSubscription sub = new FeedbinSubscription((int)obj.GetObject().GetNamedNumber("id"),
                                                      (int)obj.GetObject().GetNamedNumber("feed_id"),
                                                      values[0],
                                                      values[1],
                                                      values[2]);

                subs.Add(sub);
            }
            return subs;
        }

        private static async Task<bool> makeApiDeleteRequest(String url)
        {
            using (var client = new HttpClient())
            {
                var vault = new PasswordVault();
                var credentialList = vault.FindAllByResource("Feedbin");
                PasswordCredential credential = credentialList[0];
                credential.RetrievePassword();

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(credential.UserName + ":" + credential.Password)));

                var response = await client.DeleteAsync(new Uri(feedbinApiUrl + subscriptionsUrl));

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                    return false;
            }
        }

        private static async Task<String> makeApiPostRequest(String url, StringContent message)
        {
            using (var client = new HttpClient())
            {
                var vault = new PasswordVault();
                var credentialList = vault.FindAllByResource("Feedbin");
                PasswordCredential credential = credentialList[0];
                credential.RetrievePassword();

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(credential.UserName + ":" + credential.Password)));


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

        private static async Task<String> makeApiGetRequest(String url)
        {
            using (var client = new HttpClient())
            {
                var vault = new PasswordVault();
                var credentialList = vault.FindAllByResource("Feedbin");
                PasswordCredential credential = credentialList[0];
                credential.RetrievePassword();

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(credential.UserName + ":" + credential.Password)));

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
