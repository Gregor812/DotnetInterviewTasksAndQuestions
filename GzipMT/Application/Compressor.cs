using GzipMT.Abstractions;
using GzipMT.Cli;
using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;
using System.IO.Compression;

namespace GzipMT.Application
{
    public class Compressor :
        DataProcessor<CompressingOptions,
            UncompressedBlock,
            CompressedBlock>
    {
        public Compressor(CompressingOptions options, int bufferSize, UncompressedBlockReader reader)
            : base(options, bufferSize, reader)
        { }

        protected override CompressedBlock FillOutputBlockData(UncompressedBlock block)
        {
            CompressedBlock item;
            using (var outputMemoryStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputMemoryStream, CompressionMode.Compress, true))
                    gZipStream.Write(block.Data, 0, block.Data.Length);

                item = new CompressedBlock
                {
                    Number = block.Number,
                    Data = outputMemoryStream.ToArray()
                };
            }

            return item;
        }

        protected override void WriteOutputBlock(BinaryWriter binaryWriter, CompressedBlock block)
        {
            binaryWriter.Write(block);
        }
    }
}
