using GzipMT.DataStructures;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GzipMT.Abstractions
{
    public interface IBlockReader<out T> : IDisposable
        where T : Block
    {
        int BlocksRead { get; }
        IEnumerable<T> GetFileBlocks(CancellationToken ct);
    }
}
