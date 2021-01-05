using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;

namespace GzipMT.Application
{
    public class UncompressedBlockWriter : BlockWriter<UncompressedBlock>
    {
        /// <summary>Creates an instance of the UncompressedBlockWriter class</summary>
        /// <returns cref="UncompressedBlockWriter"></returns>
        /// <inheritdoc cref="File.OpenWrite"/>
        public static UncompressedBlockWriter GetInstance(string filename)
        {
            var inputFile = File.OpenWrite(filename);
            return new UncompressedBlockWriter(inputFile, false);
        }

        private UncompressedBlockWriter(FileStream outputFile, bool leaveOpen) :
            base(outputFile, leaveOpen)
        { }

        protected override void WriteOutputBlock(BinaryWriter binaryWriter, UncompressedBlock block)
        {
            binaryWriter.Write(block);
        }
    }
}
