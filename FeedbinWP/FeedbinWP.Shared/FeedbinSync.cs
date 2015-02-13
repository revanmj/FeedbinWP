using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
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
        static private String entriesByIdUrl = "entries.json?ids="; // oddzielane przecinkiem, np. ids=4078,4053,4324
        static private String entriesSinceUrl = "entries.json?since="; // since=2013-02-02T14:07:33.000000Z
        static private String singleEntryUrl = "entries/"; // np entries/5034.json
        
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

        static async public Task<ObservableCollection<FeedbinEntry>> getUnreadItems(String username, String password)
        {
            String data_ids = await makeApiRequest(username, password, feedbinApiUrl + unreadUrl);
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

                String data_entries = await makeApiRequest(username, password, feedbinApiUrl + entriesByIdUrl + ids);
                JsonArray tmp2 = JsonArray.Parse(data_entries);
                ObservableCollection<FeedbinEntry> entries = new ObservableCollection<FeedbinEntry>();
                foreach (JsonValue obj in tmp2)
                {
                    String[] names = {"title", "url", "author", "content", "published" };
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
                                                          DateTime.Parse(values[4]),
                                                          false);

                     Regex _htmlRegex = new Regex("<.*?>");
                     entry.summary = _htmlRegex.Replace(entry.content, string.Empty);

                    entries.Add(entry);
                }
                return entries;
            }
            else
                return null;

        }

        static private async Task<String> makeApiRequest(String username, String password, String url)
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
