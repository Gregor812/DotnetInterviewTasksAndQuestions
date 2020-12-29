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
        public int BlocksRead { get; private set; } // TODO: What if file is already read

        protected int BufferSize;

        protected BlockReader(int bufferSize)
        {
            BufferSize = bufferSize;
        }

        public IEnumerable<T> GetFileBlocks(string filename, CancellationToken ct) // TODO: move filename in ctor
        {
            using (var inputFile = File.OpenRead(filename))
            using (var binaryReader = new BinaryReader(inputFile))
            {
                while (!ct.IsCancellationRequested)
                {
                    if (TryReadInputBlock(binaryReader, out var block) && !ct.IsCancellationRequested)
                    {
                        ++BlocksRead;
                        yield return block;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        protected abstract bool TryReadInputBlock(BinaryReader binaryReader, out T block);
    }
}
