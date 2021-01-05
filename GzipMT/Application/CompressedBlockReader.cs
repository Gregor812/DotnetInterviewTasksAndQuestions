using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;

namespace GzipMT.Application
{
    public class CompressedBlockReader : BlockReader<CompressedBlock>
    {
        /// <summary>Creates an instance of the CompressedBlockReader class</summary>
        /// <returns cref="CompressedBlockReader"></returns>
        /// <inheritdoc cref="File.OpenRead"/>
        public static CompressedBlockReader GetInstance(string filename)
        {
            var inputFile = File.OpenRead(filename);
            return new CompressedBlockReader(inputFile, false);
        }

        private CompressedBlockReader(FileStream inputFile, bool leaveOpen)
            : base(inputFile, leaveOpen)
        { }

        protected override bool TryReadInputBlock(BinaryReader binaryReader, out CompressedBlock block)
        {
            block = binaryReader.ReadCompressedBlock();
            return block != null;
        }
    }
}
