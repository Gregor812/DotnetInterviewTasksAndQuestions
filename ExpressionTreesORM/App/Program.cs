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

            //for (int i = 1; i < 11; i++)
            //{
            //    await appDbContext.Products.AddAsync(
            //        new Product {InStock = 28 % i, IsForSale = i % 2 < 1 || i % 3 < 1, Name = $"{i}"}, Cts.Token);
            //}

            //await appDbContext.SaveChangesAsync(Cts.Token);

            // First
            // foreach (var p in appDbContext.Products.Where(p => p.IsForSale && p.InStock > 1))

            // Second
            // IT DOESN'T WORK BECAUSE I DON'T KNOW (MAYBE THAT'S TRUE ONLY FOR SQLITE PROVIDER?)
            // foreach (var p in appDbContext.Products.Where(p => p.IsAvailable))

            // Third
            // It works!
            //foreach (var p in appDbContext.Products.Where(new IsForSaleSpec() && !new IsInStockSpec()))

            // Fourth
            // It works as well!
            foreach (var p in appDbContext.Products.Where(Product.IsAvailable))
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
