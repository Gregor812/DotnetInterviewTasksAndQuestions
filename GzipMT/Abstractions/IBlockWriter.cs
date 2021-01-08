using System;
using System.Threading;

namespace GzipMT.Abstractions
{
    public interface IBlockWriter<T> : IDisposable
    {
        IQueue<T>[] Queues { get; }
        ManualResetEventSlim WritingDone { get; }
        void Start(CountdownEvent processingDone, CancellationToken ct);
    }
}
