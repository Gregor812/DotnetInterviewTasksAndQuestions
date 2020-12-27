using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GzipMT
{
    class Program
    {
        private const int BufferSize = 8 * 1024 * 1024;

        private static readonly int WorkerThreadsNumber = Environment.ProcessorCount;

        private static readonly BoundedConcurrentQueue<byte[]> InputQueue = new BoundedConcurrentQueue<byte[]>();
        private static readonly BoundedConcurrentQueue<byte[]> OutputQueue = new BoundedConcurrentQueue<byte[]>();

        private static readonly CancellationTokenSource Cts = new CancellationTokenSource();

        private static readonly List<Thread> Threads = new List<Thread>(2 + WorkerThreadsNumber);

        private static readonly ManualResetEventSlim ReadingDone = new ManualResetEventSlim();
        private static readonly ManualResetEventSlim ProcessingDone = new ManualResetEventSlim();
        private static readonly ManualResetEventSlim WritingDone = new ManualResetEventSlim();

        private static int _blocksRead = 0;
        private static int _blocksWritten = 0;


        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;
            Console.WriteLine($"Worker threads number is {WorkerThreadsNumber}");
            Console.WriteLine();

            var workerThreads = new List<Thread>(WorkerThreadsNumber);

            var map = new Thread(() => Map(args, Cts.Token));
            for (int i = 0; i < WorkerThreadsNumber; ++i)
            {
                workerThreads.Add(new Thread(() => ProcessInputQueue(Cts.Token)));
            }
            var reduce = new Thread(() => Reduce(args, Cts.Token));
            Threads.Add(map);
            Threads.AddRange(workerThreads);
            Threads.Add(reduce);

            var sw = new Stopwatch();
            sw.Start();

            map.Start();
            workerThreads.ForEach(t => t.Start());
            reduce.Start();

            workerThreads.ForEach(t => t.Join());
            ProcessingDone.Set();

            WaitHandle.WaitAll(new[]
            {
                ReadingDone.WaitHandle,
                ProcessingDone.WaitHandle,
                WritingDone.WaitHandle
            });
            sw.Stop();

            foreach (var thread in Threads)
            {
                thread.Join();
            }

            Console.WriteLine($"Elapsed: {sw.Elapsed:c}");
            Console.WriteLine($"Blocks read: {_blocksRead}, blocks written: {_blocksWritten}");
            Console.WriteLine("Done");
        }

        private static void Map(string[] args, CancellationToken ct)
        {
            var inputBuffer = new byte[BufferSize];

            using (var inputFile = File.OpenRead(args[1]))
            {
                var spin = new SpinWait();

                while (!ct.IsCancellationRequested && inputFile.Read(inputBuffer, 0, BufferSize) != 0)
                {
                    ++_blocksRead;
                    while (!InputQueue.TryEnqueue(inputBuffer))
                        spin.SpinOnce();
                    inputBuffer = new byte[BufferSize];
                }
                if (ct.IsCancellationRequested)
                    Console.WriteLine("Operation cancelled by user");
            }

            ReadingDone.Set();
        }

        private static void ProcessInputQueue(CancellationToken ct)
        {
            var spin = new SpinWait();

            while (!ct.IsCancellationRequested && !(InputQueue.IsEmpty && ReadingDone.IsSet))
            {
                using (var outputMemoryStream = new MemoryStream())
                using (var gZipStream = new GZipStream(outputMemoryStream, CompressionMode.Compress, true))
                {
                    if (InputQueue.TryDequeue(out var inputBuffer))
                    {
                        gZipStream.Write(inputBuffer, 0, inputBuffer.Length);
                        while (!OutputQueue.TryEnqueue(outputMemoryStream.ToArray()))
                            spin.SpinOnce();
                    }
                    else
                        spin.SpinOnce();
                }
            }
        }

        private static void Reduce(string[] args, CancellationToken ct)
        {
            using (var outputFile = File.OpenWrite(args[2]))
            {
                while (!ct.IsCancellationRequested && !(OutputQueue.IsEmpty && ProcessingDone.IsSet))
                {
                    if (OutputQueue.TryDequeue(out var buffer))
                    {
                        ++_blocksWritten;
                        outputFile.Write(buffer, 0, buffer.Length);
                    }
                    else
                        Thread.Yield();
                }
            }
            if (ct.IsCancellationRequested)
                File.Delete(args[2]);

            WritingDone.Set();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Cts.Cancel();
            foreach (var thread in Threads)
            {
                thread.Join();
            }
            Environment.Exit(-1);
        }
    }
}
