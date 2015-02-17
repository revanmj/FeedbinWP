using System;
using System.Collections.Generic;
using System.Text;

namespace FeedbinWP
{
    class FeedbinSubscription
    {
        int id { get; set; }
        int feed_id { get; set; }
        String title { get; set; }
        String feed_url { get; set; }
        String site_url { get; set; }
    }
}
