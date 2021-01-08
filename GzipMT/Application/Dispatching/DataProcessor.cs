using GzipMT.Abstractions;
using GzipMT.Application.GZip;
using GzipMT.Auxiliary;
using GzipMT.DataStructures;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GzipMT.Application.Dispatching
{
    public abstract class DataProcessor<TInput, TOutput, TWorker> : IDataProcessor
        where TInput : Block
        where TOutput : Block
        where TWorker : Worker<TInput, TOutput>
    {
        protected readonly int WorkerThreadsNumber;

        private readonly CountdownEvent _processingDone;
        private readonly IBlockReader<TInput> _reader;
        private readonly IBlockWriter<TOutput> _writer;

        // TODO: Inject logger as external dependency
        protected DataProcessor(IBlockReader<TInput> reader, IBlockWriter<TOutput> writer, int workerThreadsNumber)
        {
            WorkerThreadsNumber = workerThreadsNumber;
            _processingDone = new CountdownEvent(WorkerThreadsNumber);
            _reader = reader;
            _writer = writer;
        }

        protected abstract TWorker CreateWorker(IQueue<TInput> inputQueue, IQueue<TOutput> outputQueue);

        public int Run(CancellationToken ct)
        {
            Console.WriteLine($"Worker threads number is {WorkerThreadsNumber}");
            Console.WriteLine();

            var workers = new List<Worker<TInput, TOutput>>(WorkerThreadsNumber);

            for (int i = 0; i < WorkerThreadsNumber; ++i)
            {
                var inputQueue = _reader.Queues[i];
                var outputQueue = _writer.Queues[i];
                workers.Add(CreateWorker(inputQueue, outputQueue));
            }

            using (var sw = new ScopedStopwatch())
            {
                _reader.Start(ct);
                workers.ForEach(t => t.Start(_reader.ReadingDone, _processingDone, ct));
                _writer.Start(_processingDone, ct);
                _writer.WritingDone.Wait(ct);
                Console.WriteLine($"Elapsed: {sw.Elapsed:c}");
            }
            Console.WriteLine("Done");

            return 0;
        }

        public void WaitForExit()
        {
            _writer.WritingDone.Wait();
        }

        public void Dispose()
        {
            _processingDone?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
        }
    }
}
