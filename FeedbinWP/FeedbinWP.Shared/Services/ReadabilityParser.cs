using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace FeedbinWP.Services
{
    class ReadabilityParser
    {
        private static String readabilityUrl = "https://www.readability.com/api/content/v1/parser?url=";
        private static String readabilityToken = "&token=f36291f9960c1036f40ab453f569fddbf6729847";

        public static async Task<String> parseViaReadability(String url)
        {
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(new Uri(readabilityUrl + url + readabilityToken));
                if (response.IsSuccessStatusCode)
                {
                    String data = await response.Content.ReadAsStringAsync();
                    JsonValue obj = JsonValue.Parse(data);
                    String text = obj.GetObject().GetNamedString("content");
                    return text;
                }
                else
                    return null;
            }
        }
    }
}
