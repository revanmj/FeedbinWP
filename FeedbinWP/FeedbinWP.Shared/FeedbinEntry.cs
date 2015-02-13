using System;
using System.Collections.Generic;
using System.Text;
//using SQLite;

namespace FeedbinWP
{
    //[Table("Entries")]
    class FeedbinEntry
    {
        public FeedbinEntry(String title, String content)
        {
            this.title = title;
            this.content = content;
        }
        //[PrimaryKey]
        public int id { get; set; }
        public int feed_id { get; set; }
        public String title { get; set; }
        public String url { get; set; }
        public String author { get; set; }
        public String content { get; set; }
        public String summary { get; set; }
        public String published { get; set; }
        public DateTime created_at { get; set; }
        public bool read { get; set; }
    }
}
