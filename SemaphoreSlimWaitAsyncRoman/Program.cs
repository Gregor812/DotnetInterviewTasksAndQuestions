using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var entryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMilliseconds(10));
            using var asyncLock = new SemaphoreSlim(1, 1);
            using var mre = new ManualResetEventSlim(true);
            var syncLock = new object();
            var tasks = new ConcurrentBag<Task>();
            var datakey = 0;
            var taskKey = 0;
            var isAsync = true;
            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine($"start pool: {ThreadPool.ThreadCount},dt:{DateTime.Now}");
            Parallel.For(1, 10_000_000, i =>
            {
                tasks.Add(Task.Run(async () =>
                {
                    if (memoryCache.TryGetValue(datakey, out int value))
                    {
                        // return data from cache
                        return;
                    }

                    if (isAsync)
                    {
                        if (memoryCache.TryGetValue(taskKey, out Task task) == false)
                        {
                            await asyncLock.WaitAsync();
                            try
                            {
                                if (memoryCache.TryGetValue(taskKey, out task) == false)
                                {
                                    // http request emulation
                                    task = DoWork(memoryCache, datakey, i, entryOptions);
                                    memoryCache.Set(taskKey, task, entryOptions);
                                }
                            }
                            finally
                            {
                                asyncLock.Release();
                            }
                        }

                        await task;

                        // return data from server
                    }
                    else
                    {
                        if (memoryCache.TryGetValue(taskKey, out Task task) == false)
                        {
                            lock (syncLock)
                            {
                                if (memoryCache.TryGetValue(taskKey, out task) == false)
                                {
                                    // http request emulation
                                    task = DoWork(memoryCache, datakey, i, entryOptions);
                                    memoryCache.Set(taskKey, task, entryOptions);
                                }
                                else
                                {
                                    //Console.WriteLine($"task from cache: {i}");
                                }
                            }
                        }

                        // wait for http request result
                        await task;
                    }
                }));
            });

            await Task.WhenAll(tasks);

            sw.Stop();
            Console.WriteLine($"time: {sw.ElapsedMilliseconds}");
        }

        private static async Task DoWork(IMemoryCache memoryCache, int datakey, int i,
            MemoryCacheEntryOptions entryOptions)
        {
            await Task.Delay(1000);
            Console.WriteLine($"pool: {ThreadPool.ThreadCount},set cache: {i}, dt:{DateTime.Now}");
            memoryCache.Set(datakey, i, entryOptions);
        }
    }
}
