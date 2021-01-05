using GzipMT.Abstractions;
using GzipMT.Auxiliary;
using GzipMT.Cli;
using GzipMT.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        protected readonly IQueue<TInput>[] InputQueues;
        protected readonly IQueue<TOutput>[] OutputQueues;

        private readonly ManualResetEventSlim _readingDone;
        private readonly CountdownEvent _processingDone;
        private readonly ManualResetEventSlim _writingDone;
        private readonly IBlockReader<TInput> _reader;

        protected int BlocksRead;
        protected int BlocksWritten;

        // TODO: Inject logger as external dependency
        protected DataProcessor(TOptions options, int bufferSize,
            IBlockReader<TInput> reader)
        {
            Options = options;
            BufferSize = bufferSize;
            InputQueues = new BoundedConcurrentQueue<TInput>[Options.WorkerThreadsNumber];
            OutputQueues = new BoundedConcurrentQueue<TOutput>[Options.WorkerThreadsNumber];

            _readingDone = new ManualResetEventSlim();
            _processingDone = new CountdownEvent(Options.WorkerThreadsNumber);
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

            for (int i = 0; i < Options.WorkerThreadsNumber; ++i)
            {
                InputQueues[i] = new BoundedConcurrentQueue<TInput>(2);
                OutputQueues[i] = new BoundedConcurrentQueue<TOutput>(2);
            }

            var threads = new List<Thread>(Options.WorkerThreadsNumber + 2);
            var readInputBlocks = new Thread(() => ReadInputBlocks(ct))
            {
                Name = nameof(ReadInputBlocks)
            };
            threads.Add(readInputBlocks);

            for (int i = 0; i < Options.WorkerThreadsNumber; ++i)
            {
                var inputQueue = InputQueues[i];
                var outputQueue = OutputQueues[i];
                threads.Add(new Thread(() => ProcessInputBlocks(inputQueue, outputQueue, ct)) { Name = $"{nameof(ProcessInputBlocks)}_{i}" });
            }

            var writeOutputBlocks = new Thread(() => WriteOutputBlocks(Options.OutputFile, ct))
            {
                Name = nameof(WriteOutputBlocks)
            };
            threads.Add(writeOutputBlocks);

            using (var sw = new ScopedStopwatch())
            {
                threads.ForEach(t => t.Start());
                _writingDone.Wait(ct);
                Console.WriteLine($"Elapsed: {sw.Elapsed:c}");
            }

            Console.WriteLine($"Blocks read: {BlocksRead}, blocks written: {BlocksWritten}");
            Console.WriteLine("Done");

            return 0;
        }

        public void WaitForExit()
        {
            _writingDone.Wait();
        }

        protected void ReadInputBlocks(CancellationToken ct)
        {
            var spinner = new SpinWait();
            foreach (var block in _reader.GetFileBlocks(ct))
            {
                var nextQueue = InputQueues[BlocksRead % Options.WorkerThreadsNumber];
                while (!(nextQueue.TryEnqueue(block) || ct.IsCancellationRequested))
                {
                    spinner.SpinOnce();
                }
                ++BlocksRead;
            }
            _readingDone.Set();
        }

        private void ProcessInputBlocks(IQueue<TInput> inputQueue, IQueue<TOutput> outputQueue, CancellationToken ct)
        {
            var spinner = new SpinWait();
            while (!(_readingDone.IsSet && inputQueue.IsEmpty || ct.IsCancellationRequested))
            {
                if (inputQueue.TryDequeue(out var block))
                {
                    var outputBlock = FillOutputBlockData(block);
                    while (!(outputQueue.TryEnqueue(outputBlock) || ct.IsCancellationRequested))
                    {
                        spinner.SpinOnce();
                    }
                }
                else
                {
                    spinner.SpinOnce();
                }
            }

            _processingDone.Signal();
        }

        protected void WriteOutputBlocks(string outputFileName, CancellationToken ct)
        {
            var spinner = new SpinWait();
            using (var outputFile = File.OpenWrite(outputFileName))
            using (var binaryWriter = new BinaryWriter(outputFile))
            {
                while (!(_processingDone.IsSet && OutputQueues.All(q => q.IsEmpty) || ct.IsCancellationRequested))
                {
                    var nextQueue = OutputQueues[BlocksWritten % Options.WorkerThreadsNumber];
                    if (nextQueue.TryDequeue(out var block))
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
