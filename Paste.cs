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
        [Required, StringLength(8)]
        public string? ID { get; set; }
        public string Title { get; set; }
        [Required]
        public string Syntax { get; set; }
        [Required]
        public PasteExposure Exposure { get; set; }
        public int? Author { get; set; }
        [Required, DataType(DataType.DateTime)]
        public DateTime Date { get; set; }
    }
}
