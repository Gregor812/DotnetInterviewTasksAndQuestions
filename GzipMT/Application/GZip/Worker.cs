using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System;
using System.Threading;

namespace GzipMT.Application.GZip
{
    public abstract class Worker<TInput, TOutput>
        where TInput : Block
        where TOutput : Block
    {
        private readonly IQueue<TInput> _inputQueue;
        private readonly IQueue<TOutput> _outputQueue;

        public bool ErrorHappened { get; private set; }
        public string ErrorDescription { get; private set; }

        protected Worker(IQueue<TInput> inputQueue, IQueue<TOutput> outputQueue)
        {
            _inputQueue = inputQueue;
            _outputQueue = outputQueue;
        }

        public void Start(ManualResetEventSlim readingDone, CountdownEvent processingDone, CancellationToken ct)
        {
            var t = new Thread(() => ProcessInputQueue(readingDone, processingDone, ct))
            {
                Name = $"{nameof(ProcessInputQueue)}"
            };
            t.Start();
        }

        private void ProcessInputQueue(ManualResetEventSlim readingDone, CountdownEvent processingDone, CancellationToken ct)
        {
            try
            {
                var spinner = new SpinWait();
                while (!(readingDone.IsSet && _inputQueue.IsEmpty || ct.IsCancellationRequested))
                {
                    if (_inputQueue.TryDequeue(out var block))
                    {
                        var outputBlock = CreateOutputBlock(block);
                        while (!(_outputQueue.TryEnqueue(outputBlock) || ct.IsCancellationRequested))
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
            catch (Exception ex)
            {
                ErrorDescription = ex.Message;
                ErrorHappened = true;
            }
            finally
            {
                processingDone.Signal();
            }
        }

        protected abstract TOutput CreateOutputBlock(TInput block);
    }
}
