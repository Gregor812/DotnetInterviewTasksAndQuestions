using GzipMT.Abstractions;
using GzipMT.Cli;
using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;
using System.IO.Compression;

namespace GzipMT.Application
{
    public class Compressor : DataProcessor<CompressingOptions,
            UncompressedBlock,
            CompressedBlock>
    {
        protected override string Description => $"Compressing file {Options.InputFile}...";
        
        public Compressor(CompressingOptions options, int bufferSize,
            IBlockReader<UncompressedBlock> reader)
            : base(options, bufferSize, reader)
        { }

        protected override CompressedBlock FillOutputBlockData(UncompressedBlock block)
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

        protected override void WriteOutputBlock(BinaryWriter binaryWriter, CompressedBlock block)
        {
            binaryWriter.Write(block);
        }
    }
}
