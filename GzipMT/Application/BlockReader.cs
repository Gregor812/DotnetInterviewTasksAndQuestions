using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GzipMT.Application
{
    public abstract class BlockReader<T> : IBlockReader<T>
        where T : Block
    {
        public int BlocksRead { get; private set; }

        protected int BufferSize;

        protected BlockReader(int bufferSize)
        {
            BufferSize = bufferSize;
        }

        public IEnumerable<T> GetFileBlocks(string filename, CancellationToken ct)
        {
            using (var inputFile = File.OpenRead(filename))
            using (var binaryReader = new BinaryReader(inputFile))
            {
                while (!ct.IsCancellationRequested)
                {
                    if (TryReadInputBlock(binaryReader, out var block))
                    {
                        ++BlocksRead;
                        yield return block;
                    }
                    else break;
                }
            }
        }

        protected abstract bool TryReadInputBlock(BinaryReader binaryReader, out T block);
    }
}
