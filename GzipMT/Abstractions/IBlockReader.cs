using GzipMT.DataStructures;
using System.Collections.Generic;
using System.Threading;

namespace GzipMT.Abstractions
{
    public interface IBlockReader<out T>
        where T : Block
    {
        int BlocksRead { get; }
        IEnumerable<T> GetFileBlocks(string filename, CancellationToken ct);
    }
}
