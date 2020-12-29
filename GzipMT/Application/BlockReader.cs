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

        private readonly FileStream _inputFile;
        protected int BufferSize;

        protected BlockReader(FileStream inputFile, int bufferSize)
        {
            _inputFile = inputFile;
            BufferSize = bufferSize;
        }

        public IEnumerable<T> GetFileBlocks(string filename, CancellationToken ct) // TODO: move filename in ctor
        {
            using (var binaryReader = new BinaryReader(_inputFile))
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

        public void Dispose()
        {
            _inputFile?.Dispose();
        }

        protected abstract bool TryReadInputBlock(BinaryReader binaryReader, out T block);
    }
}
