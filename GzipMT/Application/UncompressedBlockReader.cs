using GzipMT.DataStructures;
using System.IO;

namespace GzipMT.Application
{
    public class UncompressedBlockReader : BlockReader<UncompressedBlock>
    {
        public UncompressedBlockReader(int bufferSize)
            : base(bufferSize)
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
