using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System.IO;
using System.Text;
using System.Threading;

namespace GzipMT.Application
{
    public abstract class BlockWriter<T> : IBlockWriter<T>
        where T : Block
    {
        private readonly FileStream _outputFile;
        private readonly bool _leaveOpen;

        protected BlockWriter(FileStream outputFile, bool leaveOpen)
        {
            _outputFile = outputFile;
            _leaveOpen = leaveOpen;
        }

        public void WriteFileBlock(T block, CancellationToken ct)
        {
            using (var binaryWriter = new BinaryWriter(_outputFile, Encoding.Default, true))
            {
                WriteOutputBlock(binaryWriter, block);
            }
        }

        public void Dispose()
        {
            if (!_leaveOpen)
            {
                _outputFile?.Dispose();
            }
        }

        protected abstract void WriteOutputBlock(BinaryWriter binaryWriter, T block);
    }
}
