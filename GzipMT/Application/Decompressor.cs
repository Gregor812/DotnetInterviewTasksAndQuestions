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
        DataProcessor<DecompressingOptions,
            CompressedBlock,
            UncompressedBlock>
    {
        protected override string Description => $"Decompressing file {Options.InputFile}...";

        public Decompressor(DecompressingOptions options, int bufferSize,
            IBlockReader<CompressedBlock> reader)
            : base(options, bufferSize, reader)
        { }

        protected override UncompressedBlock FillOutputBlockData(CompressedBlock block)
        {
            var buffer = new byte[BufferSize];
            int readBytes;

            using (var inputMemory = new MemoryStream(block.Data))
            using (var gZipStream = new GZipStream(inputMemory, CompressionMode.Decompress))
            {
                readBytes = gZipStream.Read(buffer, 0, BufferSize);
            }

            var item = new UncompressedBlock
            {
                Data = readBytes == BufferSize ? buffer : buffer.Take(readBytes).ToArray()
            };
            return item;
        }

        protected override void WriteOutputBlock(BinaryWriter binaryWriter, UncompressedBlock block)
        {
            binaryWriter.Write(block);
        }
    }
}
