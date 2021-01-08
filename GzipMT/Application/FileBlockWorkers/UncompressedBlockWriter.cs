using GzipMT.Abstractions;
using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;

namespace GzipMT.Application.FileBlockWorkers
{
    public class UncompressedBlockWriter : BlockWriter<UncompressedBlock>
    {
        /// <summary>Creates an instance of the UncompressedBlockWriter class</summary>
        /// <returns cref="UncompressedBlockWriter"></returns>
        /// <inheritdoc cref="File.OpenWrite"/>
        public static UncompressedBlockWriter GetInstance(string filename, IQueue<UncompressedBlock>[] queues)
        {
            var fileToWrite = File.OpenWrite(filename);
            return new UncompressedBlockWriter(fileToWrite, queues);
        }

        public UncompressedBlockWriter(FileStream fileToWrite, IQueue<UncompressedBlock>[] queues)
            : base(fileToWrite, queues)
        { }

        protected override void WriteOutputBlock(BinaryWriter binaryWriter, UncompressedBlock block)
        {
            binaryWriter.Write(block);
        }
    }
}
