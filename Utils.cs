using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevBin {
    public class Utils {
        public static string RandomString(int length) {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_!?=()[]{}<>/,.;:";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
