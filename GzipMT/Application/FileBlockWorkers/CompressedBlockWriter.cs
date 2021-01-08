using GzipMT.Abstractions;
using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;

namespace GzipMT.Application.FileBlockWorkers
{
    public class CompressedBlockWriter : BlockWriter<CompressedBlock>
    {
        /// <summary>Creates an instance of the CompressedBlockWriter class</summary>
        /// <returns cref="CompressedBlockWriter"></returns>
        /// <inheritdoc cref="File.OpenWrite"/>
        public static CompressedBlockWriter GetInstance(string filename, IQueue<CompressedBlock>[] queues)
        {
            var fileToWrite = File.OpenWrite(filename);
            return new CompressedBlockWriter(fileToWrite, queues);
        }

        public CompressedBlockWriter(FileStream fileToWrite, IQueue<CompressedBlock>[] queues)
            : base(fileToWrite, queues)
        { }

        protected override void WriteOutputBlock(BinaryWriter binaryWriter, CompressedBlock block)
        {
            binaryWriter.Write(block);
        }
    }
}
