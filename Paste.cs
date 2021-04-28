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

        [Required, StringLength(8)]
        public string ID { get; set; }
        public string Title { get; set; }
        [Required]
        public string Syntax { get; set; }
        [Required]
        public PasteExposure Exposure { get; set; }
        public int? Author { get; set; }
        [Required, DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        public static string GeneratePasteID() {
            string code = "";

            string alpha = "abcdefghijklmnopqrstuvwxyz";

            Random random = new Random();

            for ( int i = 0; i < 8; i++ ) {
                string ch = alpha[random.Next(0, alpha.Length)].ToString();
                if ( random.Next(0, 1) > 0 ) {
                    ch = ch.ToUpper();
                }
                code += ch;
            }

            return code;
        }

    }
}
