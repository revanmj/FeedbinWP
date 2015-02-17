using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace FeedbinWP.Data
{
    [Table("Entries")]
    class FeedbinEntry
    {
        public FeedbinEntry() {}

        public FeedbinEntry(int id, int fid, String title, String url, String author, String content, DateTime published)
        {
            this.id = id;
            this.feed_id = fid;
            this.title = title;
            this.url = url;
            this.author = author;
            this.content = content;
            this.published = published;
            this.read = true;
            this.starred = false;
            this.recent = false;
        }

        [PrimaryKey]
        public int id { get; set; }
        public int feed_id { get; set; }
        public String title { get; set; }
        public String url { get; set; }
        public String author { get; set; }
        public String content { get; set; }
        public String summary { get; set; }
        public DateTime published { get; set; }
        public bool read { get; set; }
        public bool starred { get; set; }
        public bool recent { get; set; }
    }
}
