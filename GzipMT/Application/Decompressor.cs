using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GzipMT.Application
{
    public class Decompressor :
        DataProcessor<CompressedBlock, UncompressedBlock>
    {
        private readonly int _bufferSizeBytes;

        public Decompressor(IBlockReader<CompressedBlock> reader, IBlockWriter<UncompressedBlock> writer,
            int bufferSizeBytes, int workerThreadsNumber)
            : base(reader, writer, workerThreadsNumber)
        {
            _bufferSizeBytes = bufferSizeBytes;
        }

        protected override UncompressedBlock CreateOutputBlock(CompressedBlock block)
        {
            var buffer = new byte[_bufferSizeBytes];
            int readBytes;

            using (var inputMemory = new MemoryStream(block.Data))
            using (var gZipStream = new GZipStream(inputMemory, CompressionMode.Decompress))
            {
                readBytes = gZipStream.Read(buffer, 0, _bufferSizeBytes);
            }

            var item = new UncompressedBlock
            {
                Data = readBytes == _bufferSizeBytes ? buffer : buffer.Take(readBytes).ToArray()
            };
            return item;
        }
    }
}
