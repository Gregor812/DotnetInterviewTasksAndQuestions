using GzipMT.Abstractions;
using GzipMT.Cli;
using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GzipMT.Application
{
    public class Decompressor :
        DataProcessor<CompressedBlock, UncompressedBlock>
    {
        private readonly int _bufferSize;

        public Decompressor(int workerThreadsNumber, IBlockReader<CompressedBlock> reader,
            IBlockWriter<UncompressedBlock> writer, int bufferSize)
            : base(workerThreadsNumber, reader, writer)
        {
            _bufferSize = bufferSize;
        }

        protected override UncompressedBlock CreateOutputBlock(CompressedBlock block)
        {
            var buffer = new byte[_bufferSize];
            int readBytes;

            using (var inputMemory = new MemoryStream(block.Data))
            using (var gZipStream = new GZipStream(inputMemory, CompressionMode.Decompress))
            {
                readBytes = gZipStream.Read(buffer, 0, _bufferSize);
            }

            var item = new UncompressedBlock
            {
                Data = readBytes == _bufferSize ? buffer : buffer.Take(readBytes).ToArray()
            };
            return item;
        }
    }
}
