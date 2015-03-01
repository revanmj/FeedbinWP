using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace FeedbinWP.Data
{
    [Table("Subscriptions")]
    class FeedbinSubscription
    {
        public FeedbinSubscription() { }

        public FeedbinSubscription(int id, int fid, String title, String feed_url, String site_url)
        {
            this.id = id;
            this.feed_id = fid;
            this.title = title;
            this.feed_url = feed_url;
            this.site_url = site_url;
        }

        [PrimaryKey]
        public int id { get; set; }
        public int feed_id { get; set; }
        public String title { get; set; }
        public String feed_url { get; set; }
        public String site_url { get; set; }
    }
}
