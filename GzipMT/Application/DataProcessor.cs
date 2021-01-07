using GzipMT.Abstractions;
using GzipMT.Auxiliary;
using GzipMT.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GzipMT.Application
{
    public abstract class DataProcessor<TInput, TOutput> : IDataProcessor
        where TInput : Block
        where TOutput : Block
    {
        protected readonly int WorkerThreadsNumber;
        protected readonly IQueue<TInput>[] InputQueues;
        protected readonly IQueue<TOutput>[] OutputQueues;

        private readonly ManualResetEventSlim _readingDone;
        private readonly CountdownEvent _processingDone;
        private readonly ManualResetEventSlim _writingDone;
        private readonly IBlockReader<TInput> _reader;
        private readonly IBlockWriter<TOutput> _writer;

        protected int BlocksRead;
        protected int BlocksWritten;

        // TODO: Inject logger as external dependency
        protected DataProcessor(IBlockReader<TInput> reader, IBlockWriter<TOutput> writer, int workerThreadsNumber)
        {
            WorkerThreadsNumber = workerThreadsNumber;
            InputQueues = new BoundedConcurrentQueue<TInput>[WorkerThreadsNumber];
            OutputQueues = new BoundedConcurrentQueue<TOutput>[WorkerThreadsNumber];

            _readingDone = new ManualResetEventSlim();
            _processingDone = new CountdownEvent(WorkerThreadsNumber);
            _writingDone = new ManualResetEventSlim();
            _reader = reader;
            _writer = writer;
        }

        protected abstract TOutput CreateOutputBlock(TInput block);

        public int Run(CancellationToken ct)
        {
            Console.WriteLine($"Worker threads number is {WorkerThreadsNumber}");
            Console.WriteLine();

            for (int i = 0; i < WorkerThreadsNumber; ++i)
            {
                InputQueues[i] = new BoundedConcurrentQueue<TInput>(2);
                OutputQueues[i] = new BoundedConcurrentQueue<TOutput>(2);
            }

            var threads = new List<Thread>(WorkerThreadsNumber + 2);
            var readInputBlocks = new Thread(() => ReadInputBlocks(ct))
            {
                Name = nameof(ReadInputBlocks)
            };
            threads.Add(readInputBlocks);

            for (int i = 0; i < WorkerThreadsNumber; ++i)
            {
                var inputQueue = InputQueues[i];
                var outputQueue = OutputQueues[i];
                threads.Add(new Thread(() => ProcessInputBlocks(inputQueue, outputQueue, ct)) { Name = $"{nameof(ProcessInputBlocks)}_{i}" });
            }

            var writeOutputBlocks = new Thread(() => WriteOutputBlocks(ct))
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
                var nextQueue = InputQueues[BlocksRead % WorkerThreadsNumber];
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
                    var outputBlock = CreateOutputBlock(block);
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

        protected void WriteOutputBlocks(CancellationToken ct)
        {
            var spinner = new SpinWait();
            while (!(_processingDone.IsSet && OutputQueues.All(q => q.IsEmpty) || ct.IsCancellationRequested))
            {
                var nextQueue = OutputQueues[BlocksWritten % WorkerThreadsNumber];
                if (nextQueue.TryDequeue(out var block))
                {
                    _writer.WriteFileBlock(block, ct);
                    ++BlocksWritten;
                }
                else
                {
                    spinner.SpinOnce();
                }
            }
            _writingDone.Set();
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _readingDone?.Dispose();
                _processingDone?.Dispose();
                _writingDone?.Dispose();
                _reader?.Dispose();
                _writer?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
