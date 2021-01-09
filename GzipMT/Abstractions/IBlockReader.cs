using GzipMT.DataStructures;
using System;
using System.Threading;

namespace GzipMT.Abstractions
{
    public interface IBlockReader<T> : IDisposable
        where T : Block
    {
        IQueue<T>[] Queues { get; }
        ManualResetEventSlim ReadingDone { get; }
        void Start(CancellationToken ct);
    }
}
