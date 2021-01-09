using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System.IO;
using System.IO.Compression;

namespace GzipMT.Application.GZip
{
    public class CompressionWorker : Worker<UncompressedBlock, CompressedBlock>
    {
        public CompressionWorker(IQueue<UncompressedBlock> inputQueue, IQueue<CompressedBlock> outputQueue)
            : base(inputQueue, outputQueue)
        { }

        protected override CompressedBlock CreateOutputBlock(UncompressedBlock block)
        {
            using (var outputMemoryStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputMemoryStream, CompressionMode.Compress, true))
                {
                    gZipStream.Write(block.Data, 0, block.Data.Length);
                }

                return new CompressedBlock
                {
                    Data = outputMemoryStream.ToArray() // TODO: Check if there is a possibility to store the memory stream
                };
            }
        }
    }
}
