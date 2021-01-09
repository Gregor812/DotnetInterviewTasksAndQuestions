using GzipMT.Abstractions;
using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;

namespace GzipMT.Application.FileBlockWorkers
{
    public class CompressedBlockReader : BlockReader<CompressedBlock>
    {
        /// <summary>Creates an instance of the CompressedBlockReader class</summary>
        /// <returns cref="CompressedBlockReader"></returns>
        /// <inheritdoc cref="File.OpenRead"/>
        public static CompressedBlockReader GetInstance(string filename, IQueue<CompressedBlock>[] queues)
        {
            var inputFile = File.OpenRead(filename);
            var fileInfo = new FileInfo(filename);
            return new CompressedBlockReader(inputFile, fileInfo, queues);
        }

        private CompressedBlockReader(FileStream fileToRead, FileInfo fileInfo, IQueue<CompressedBlock>[] queues)
            : base(fileToRead, fileInfo, queues)
        { }

        protected override bool TryReadInputBlock(BinaryReader binaryReader, out CompressedBlock block)
        {
            block = binaryReader.ReadCompressedBlock();
            return block != null;
        }
    }
}
