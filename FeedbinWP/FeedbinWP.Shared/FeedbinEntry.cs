using System;
using System.Collections.Generic;
using System.Text;
//using SQLite;

namespace FeedbinWP
{
    //[Table("Entries")]
    class FeedbinEntry
    {
        public FeedbinEntry() {}

        public FeedbinEntry(String title, String content)
        {
            this.title = title;
            this.content = content;
        }

        public FeedbinEntry(int id, int fid, String title, String url, String author, String content, DateTime published, bool read)
        {
            this.id = id;
            this.feed_id = fid;
            this.title = title;
            this.url = url;
            this.author = author;
            this.content = content;
            this.published = published;
            this.read = read;
        }
        //[PrimaryKey]
        public int id { get; set; }
        public int feed_id { get; set; }
        public String title { get; set; }
        public String url { get; set; }
        public String author { get; set; }
        public String content { get; set; }
        public String summary { get; set; }
        public DateTime published { get; set; }
        public bool read { get; set; }
    }
}
