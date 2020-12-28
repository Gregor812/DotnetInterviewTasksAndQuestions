using GzipMT.Abstractions;
using GzipMT.Cli;
using GzipMT.DataStructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GzipMT.Application
{
    public abstract class DataProcessor<TOptions, TInput, TOutput> : IDataProcessor
        where TOptions : ProcessingOptions
        where TInput : Block
        where TOutput : Block
    {
        protected readonly int WorkerThreadsNumber = Environment.ProcessorCount;

        protected readonly TOptions Options;

        protected readonly int BufferSize;

        protected readonly BoundedConcurrentQueue<TInput> InputQueue;
        protected readonly BoundedConcurrentQueue<TOutput> OutputQueue;

        protected readonly List<Thread> Threads;

        protected readonly ManualResetEventSlim ReadingDone;
        protected readonly ManualResetEventSlim ProcessingDone;
        protected readonly ManualResetEventSlim WritingDone;

        protected int BlocksWritten;

        private readonly IBlockReader<TInput> _reader;

        protected DataProcessor(TOptions options, int bufferSize, IBlockReader<TInput> reader)
        {
            BufferSize = bufferSize;

            Options = options;

            InputQueue = new BoundedConcurrentQueue<TInput>();
            OutputQueue = new BoundedConcurrentQueue<TOutput>();

            Threads = new List<Thread>(WorkerThreadsNumber + 2);

            ReadingDone = new ManualResetEventSlim();
            ProcessingDone = new ManualResetEventSlim();
            WritingDone = new ManualResetEventSlim();

            _reader = reader;
            BufferSize = bufferSize;
        }

        protected abstract TOutput FillOutputBlockData(TInput block);
        protected abstract void WriteOutputBlock(BinaryWriter binaryWriter, TOutput block);

        public int Run(CancellationToken ct)
        {
            Console.WriteLine($"Compressing file {Options.InputFile}...");
            Console.WriteLine($"Worker threads number is {WorkerThreadsNumber}");
            Console.WriteLine();

            var workerThreads = new List<Thread>(WorkerThreadsNumber);

            var readInputBlocks = new Thread(() => ReadInputBlocks(Options.InputFile, ct))
            {
                Name = nameof(ReadInputBlocks)
            };
            for (int i = 0; i < WorkerThreadsNumber; ++i)
            {
                workerThreads.Add(new Thread(() => ProcessInputBlocks(ct)) { Name = nameof(ProcessInputBlocks) });
            }

            var writeOutputBlocks = new Thread(() => WriteOutputBlocks(Options.OutputFile, ct))
            {
                Name = nameof(WriteOutputBlocks)
            };

            Threads.Add(readInputBlocks);
            Threads.AddRange(workerThreads);
            Threads.Add(writeOutputBlocks);

            var sw = new Stopwatch();
            sw.Start();

            readInputBlocks.Start();
            workerThreads.ForEach(t => t.Start());
            writeOutputBlocks.Start();
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
            Console.WriteLine($"Blocks read: {_reader.BlocksRead}, blocks written: {BlocksWritten}");
            Console.WriteLine("Done");

            return 0;
        }

        public void WaitForExit()
        {
            foreach (var thread in Threads)
            {
                thread.Join();
            }
        }

        protected void ReadInputBlocks(string inputFileName, CancellationToken ct)
        {
            var spin = new SpinWait();
            foreach (var block in _reader.GetFileBlocks(inputFileName, ct))
            {
                while (!InputQueue.TryEnqueue(block))
                    spin.SpinOnce();
            }

            ReadingDone.Set();
        }

        private void ProcessInputBlocks(CancellationToken ct)
        {
            var spin = new SpinWait();

            while (!ct.IsCancellationRequested && !(ReadingDone.IsSet && InputQueue.IsEmpty))
            {
                if (InputQueue.TryDequeue(out var block))
                {
                    var outputBlock = FillOutputBlockData(block);
                    while (!OutputQueue.TryEnqueue(outputBlock))
                        spin.SpinOnce();
                }
                else
                    spin.SpinOnce();
            }
        }

        protected void WriteOutputBlocks(string outputFileName, CancellationToken ct)
        {
            using (var outputFile = File.OpenWrite(outputFileName))
            using (var binaryWriter = new BinaryWriter(outputFile))
            {
                while (!ct.IsCancellationRequested && !(OutputQueue.IsEmpty && ProcessingDone.IsSet))
                {
                    if (OutputQueue.TryDequeue(out var block))
                    {
                        WriteOutputBlock(binaryWriter, block);
                        ++BlocksWritten;
                    }
                    else
                        Thread.Yield();
                }
            }
            if (ct.IsCancellationRequested)
                File.Delete(outputFileName);

            WritingDone.Set();
        }
    }
}
