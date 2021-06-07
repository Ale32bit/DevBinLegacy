using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace DevBin {
    public class Program {
        public static int BCryptCost;
        public static void Main(string[] args) {
            Console.WriteLine("Starting BCrypt benchmark...");
            BCryptCost = Benchmark.GetOptimalBCryptCost();

            Console.WriteLine($"Optimal BCrypt Cost: {BCryptCost}");

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => {
                    logging.ClearProviders();
                    logging.AddConsole();

                })
                .ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
