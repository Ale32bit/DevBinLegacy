using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin {
    public class Paste {
        public enum PasteExposure {
            Public = 0,
            Unlisted = 1,
            Private = 2,
            Encrypted = 3,
        }

#nullable enable
        public string ID { get; set; }
        public string Title { get; set; }
        public string Syntax { get; set; }
        public PasteExposure Exposure { get; set; }
        public int? Author { get; set; }
        public DateTime Date { get; set; }

        public static string TimeAgo(DateTime date) {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.Now.Ticks - date.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if ( delta < 1 * MINUTE )
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if ( delta < 2 * MINUTE )
                return "a minute ago";

            if ( delta < 45 * MINUTE )
                return ts.Minutes + " minutes ago";

            if ( delta < 90 * MINUTE )
                return "an hour ago";

            if ( delta < 24 * HOUR )
                return ts.Hours + " hours ago";

            if ( delta < 48 * HOUR )
                return "yesterday";

            if ( delta < 30 * DAY )
                return ts.Days + " days ago";

            if ( delta < 12 * MONTH ) {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            } else {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }
    }
}
