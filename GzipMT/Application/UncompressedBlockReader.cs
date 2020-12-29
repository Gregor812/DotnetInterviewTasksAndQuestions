using GzipMT.DataStructures;
using System.IO;

namespace GzipMT.Application
{
    public class UncompressedBlockReader : BlockReader<UncompressedBlock>
    {
        /// <summary>Creates an instance of the UncompressedBlockReader class</summary>
        /// <returns cref="UncompressedBlockReader"></returns>
        /// <inheritdoc cref="File.OpenRead"/>
        public static UncompressedBlockReader GetInstance(string filename, int bufferSize)
        {
            var inputFile = File.OpenRead(filename);
            return new UncompressedBlockReader(inputFile, bufferSize);
        }

        private UncompressedBlockReader(FileStream inputFile, int bufferSize)
            : base(inputFile, bufferSize)
        { }

        protected override bool TryReadInputBlock(BinaryReader binaryReader, out UncompressedBlock block)
        {
            var bytes = binaryReader.ReadBytes(BufferSize);
            if (bytes.Length == 0)
            {
                block = default;
                return false;
            }

            block = new UncompressedBlock
            {
                Number = BlocksRead,
                Data = bytes
            };
            return true;
        }
    }
}
