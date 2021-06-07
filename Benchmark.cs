using System;
using System.Diagnostics;
using System.Linq;

namespace DevBin {
    public class Benchmark {
        public static int GetOptimalBCryptCost(int minTimeTarget = 100) {
            long timeTaken;
            int cost = 3;

            var password = RandomString(32);

            do {
                cost++;
                if ( cost > 31 ) return 31;
                var sw = Stopwatch.StartNew();

                BCrypt.Net.BCrypt.HashPassword(password, workFactor: cost);

                sw.Stop();
                timeTaken = sw.ElapsedMilliseconds;
            } while ( timeTaken < minTimeTarget );

            return cost;
        }

        public static string RandomString(int length) {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_!?=()[]{}<>/,.;:";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    

}
