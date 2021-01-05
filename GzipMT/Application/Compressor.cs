using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System.IO;
using System.IO.Compression;

namespace GzipMT.Application
{
    public class Compressor : DataProcessor<UncompressedBlock, CompressedBlock>
    {
        public Compressor(int workerThreadsNumber, IBlockReader<UncompressedBlock> reader,
            IBlockWriter<CompressedBlock> writer)
            : base(workerThreadsNumber, reader, writer)
        { }

        protected override CompressedBlock CreateOutputBlock(UncompressedBlock block)
        {
            using (var outputMemoryStream = new MemoryStream()) // TODO: profile allocations
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
