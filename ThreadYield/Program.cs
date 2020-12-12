using System;
using System.Linq;
using System.Threading;

namespace ThreadYield
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CancelKeyPress += CancelKeyPress;
            Console.WriteLine("Press Ctrl + C to stop");
            new Thread(Do).Start();
            while (true)
            {
                
            }
        }

        static void Do(object state)
        {
            foreach (var i in Enumerable.Range(0, 1000))
            {
                Console.WriteLine($"{i}: Thread id before yield: {Thread.CurrentThread.ManagedThreadId}");
                Thread.Yield();
                Console.WriteLine($"{i}: Thread id after yield: {Thread.CurrentThread.ManagedThreadId}");
            }
        }

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
