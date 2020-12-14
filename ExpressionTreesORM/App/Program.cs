using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models;

namespace App
{
    class Program
    {
        private static readonly CancellationTokenSource Cts = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Press Ctrl+C to cancel");
            Console.CancelKeyPress += Console_CancelKeyPress;

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = config.GetConnectionString("Default");
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connectionString)
                .Options;

            await using var appDbContext = new AppDbContext(options);

            // First
            // foreach (var p in appDbContext.Products.Where(p => p.IsForSale && p.InStock > 1))

            // Second
            // IT DOESN'T WORK BECAUSE I DON'T KNOW (MAYBE THAT'S TRUE ONLY FOR SQLITE PROVIDER?)
            // foreach (var p in appDbContext.Products.Where(p => p.IsAvailable))

            foreach (var p in appDbContext.Products.Where(p => p.IsAvailable))
            {
                Console.WriteLine($"{p.Id}: {p.Name} | {p.InStock} | {p.IsForSale}");
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Operation has been cancelled by user");
            Cts.Cancel();
            while (!Cts.IsCancellationRequested)
            { }
            Environment.Exit(-1);
        }
    }
}
