using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GzipMT.Application.GZip
{
    public class DecompressionWorker : Worker<CompressedBlock, UncompressedBlock>
    {
        private readonly int _bufferSizeBytes;

        public DecompressionWorker(IQueue<CompressedBlock> inputQueue,
            IQueue<UncompressedBlock> outputQueue, int bufferSizeBytes)
            : base(inputQueue, outputQueue)
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
