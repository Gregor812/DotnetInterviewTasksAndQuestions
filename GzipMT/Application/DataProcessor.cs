using GzipMT.Abstractions;
using GzipMT.Auxiliary;
using GzipMT.Cli;
using GzipMT.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GzipMT.Application
{
    public abstract class DataProcessor<TOptions, TInput, TOutput> : IDataProcessor
        where TOptions : ProcessingOptions
        where TInput : Block
        where TOutput : Block
    {
        protected abstract string Description { get; }

        protected readonly TOptions Options;
        protected readonly int BufferSize;
        protected readonly BoundedConcurrentQueue<TInput> InputQueue;
        protected readonly BoundedConcurrentQueue<TOutput> OutputQueue;

        private readonly List<Thread> _threads;
        private readonly ManualResetEventSlim _readingDone;
        private readonly ManualResetEventSlim _processingDone;
        private readonly ManualResetEventSlim _writingDone;
        private readonly IBlockReader<TInput> _reader;

        protected int BlocksWritten;

        // TODO: Inject logger as external dependency
        protected DataProcessor(TOptions options, int bufferSize,
            IBlockReader<TInput> reader)
        {
            Options = options;
            BufferSize = bufferSize;
            InputQueue = new BoundedConcurrentQueue<TInput>();
            OutputQueue = new BoundedConcurrentQueue<TOutput>();

            _threads = new List<Thread>(options.WorkerThreadsNumber + 2);
            _readingDone = new ManualResetEventSlim();
            _processingDone = new ManualResetEventSlim();
            _writingDone = new ManualResetEventSlim();
            _reader = reader;
        }

        protected abstract TOutput FillOutputBlockData(TInput block);
        protected abstract void WriteOutputBlock(BinaryWriter binaryWriter, TOutput block);

        public int Run(CancellationToken ct)
        {
            Console.WriteLine(Description);
            Console.WriteLine($"Worker threads number is {Options.WorkerThreadsNumber}");
            Console.WriteLine();

            var workerThreads = new List<Thread>(Options.WorkerThreadsNumber);

            var readInputBlocks = new Thread(() => ReadInputBlocks(Options.InputFile, ct))
            {
                Name = nameof(ReadInputBlocks)
            };
            for (int i = 0; i < Options.WorkerThreadsNumber; ++i)
            {
                workerThreads.Add(new Thread(() => ProcessInputBlocks(ct)) { Name = nameof(ProcessInputBlocks) });
            }
            var writeOutputBlocks = new Thread(() => WriteOutputBlocks(Options.OutputFile, ct))
            {
                Name = nameof(WriteOutputBlocks)
            };

            _threads.Add(readInputBlocks);
            _threads.AddRange(workerThreads);
            _threads.Add(writeOutputBlocks);

            using (var sw = new ScopedStopwatch())
            {
                _threads.ForEach(t => t.Start());
                workerThreads.ForEach(t => t.Join());
                _processingDone.Set();

                _writingDone.Wait(ct);
                Console.WriteLine($"Elapsed: {sw.Elapsed:c}");
            }

            Console.WriteLine($"Blocks read: {_reader.BlocksRead}, blocks written: {BlocksWritten}");
            Console.WriteLine("Done");

            return 0;
        }

        public void WaitForExit()
        {
            _writingDone.Wait();
        }

        protected void ReadInputBlocks(string inputFileName, CancellationToken ct) // TODO: move filename to ctor
        {
            var spinner = new SpinWait();
            foreach (var block in _reader.GetFileBlocks(inputFileName, ct))
            {
                while (!(InputQueue.TryEnqueue(block) || ct.IsCancellationRequested))
                {
                    spinner.SpinOnce();
                }
            }
            _readingDone.Set();
        }

        private void ProcessInputBlocks(CancellationToken ct)
        {
            var spinner = new SpinWait();
            while (!(_readingDone.IsSet && InputQueue.IsEmpty || ct.IsCancellationRequested))
            {
                if (InputQueue.TryDequeue(out var block))
                {
                    var outputBlock = FillOutputBlockData(block);
                    while (!(OutputQueue.TryEnqueue(outputBlock) || ct.IsCancellationRequested))
                    {
                        spinner.SpinOnce();
                    }
                }
                else
                {
                    spinner.SpinOnce();
                }
            }
        }

        protected void WriteOutputBlocks(string outputFileName, CancellationToken ct)
        {
            var spinner = new SpinWait();
            using (var outputFile = File.OpenWrite(outputFileName))
            using (var binaryWriter = new BinaryWriter(outputFile))
            {
                while (!(_processingDone.IsSet && OutputQueue.IsEmpty || ct.IsCancellationRequested))
                {
                    if (OutputQueue.TryDequeue(out var block))
                    {
                        WriteOutputBlock(binaryWriter, block);
                        ++BlocksWritten;
                    }
                    else
                    {
                        spinner.SpinOnce();
                    }
                }
            }

            if (ct.IsCancellationRequested)
            {
                File.Delete(outputFileName);
            }
            _writingDone.Set();
        }
    }
}
