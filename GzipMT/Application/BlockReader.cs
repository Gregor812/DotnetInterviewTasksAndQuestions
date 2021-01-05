using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace GzipMT.Application
{
    public abstract class BlockReader<T> : IBlockReader<T>
        where T : Block
    {
        private readonly FileStream _inputFile;
        private readonly bool _leaveOpen;
        protected int BufferSize;

        protected BlockReader(FileStream inputFile, int bufferSize, bool leaveOpen)
        {
            _inputFile = inputFile;
            BufferSize = bufferSize;
            _leaveOpen = leaveOpen;
        }

        public IEnumerable<T> GetFileBlocks(CancellationToken ct)
        {
            using (var binaryReader = new BinaryReader(_inputFile, Encoding.Default, _leaveOpen))
            {
                while (!ct.IsCancellationRequested)
                {
                    if (TryReadInputBlock(binaryReader, out var block) && !ct.IsCancellationRequested)
                    {
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
            if (!_leaveOpen)
            {
                _inputFile?.Dispose();
            }
        }

        protected abstract bool TryReadInputBlock(BinaryReader binaryReader, out T block);
    }
}
