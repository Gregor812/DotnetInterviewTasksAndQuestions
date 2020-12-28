using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;

namespace GzipMT.Application
{
    public class CompressedBlockReader : BlockReader<CompressedBlock>
    {
        public CompressedBlockReader(int bufferSize) : base(bufferSize)
        { }

        protected override bool TryReadInputBlock(BinaryReader binaryReader, out CompressedBlock block)
        {
            block = binaryReader.ReadCompressedBlock();
            return block != null;
        }
    }
}
