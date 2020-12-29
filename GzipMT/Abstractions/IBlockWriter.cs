using System.Threading;

namespace GzipMT.Abstractions
{
    public interface IBlockWriter<in T>
    {
        int BlocksWritten { get; }
        void WriteFileBlock(T block, string filename, CancellationToken ct);
    }
}
