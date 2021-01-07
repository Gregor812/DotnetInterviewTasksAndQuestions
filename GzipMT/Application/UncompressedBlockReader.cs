using GzipMT.DataStructures;
using System.IO;

namespace GzipMT.Application
{
    public class UncompressedBlockReader : BlockReader<UncompressedBlock>
    {
        private readonly int _bufferSizeBytes;

        /// <summary>Creates an instance of the UncompressedBlockReader class</summary>
        /// <returns cref="UncompressedBlockReader"></returns>
        /// <inheritdoc cref="File.OpenRead"/>
        public static UncompressedBlockReader GetInstance(string filename, int bufferSizeBytes)
        {
            var inputFile = File.OpenRead(filename);
            return new UncompressedBlockReader(inputFile, bufferSizeBytes, false);
        }

        private UncompressedBlockReader(FileStream inputFile, int bufferSizeBytes, bool leaveOpen)
            : base(inputFile, leaveOpen)
        {
            _bufferSizeBytes = bufferSizeBytes;
        }

        protected override bool TryReadInputBlock(BinaryReader binaryReader, out UncompressedBlock block)
        {
            var bytes = binaryReader.ReadBytes(_bufferSizeBytes);
            if (bytes.Length == 0)
            {
                block = default;
                return false;
            }

            block = new UncompressedBlock
            {
                Data = bytes
            };
            return true;
        }
    }
}
