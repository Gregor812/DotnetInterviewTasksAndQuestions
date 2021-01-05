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
        protected int BufferSize;

        protected BlockWriter(FileStream outputFile, int bufferSize)
        {
            _outputFile = outputFile;
            BufferSize = bufferSize;
        }

        public void WriteFileBlock(T block, string filename, CancellationToken ct)
        {
            using (var binaryWriter = new BinaryWriter(_outputFile, Encoding.Default, true))
            {
                WriteOutputBlock(binaryWriter, block);
            }
        }

        public void Dispose()
        {
            _outputFile?.Dispose();
        }

        protected abstract void WriteOutputBlock(BinaryWriter binaryWriter, T block);
    }
}
