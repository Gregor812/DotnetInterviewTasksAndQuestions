using GzipMT.DataStructures;
using GzipMT.Extensions;
using System.IO;

namespace GzipMT.Application
{
    public class CompressedBlockWriter : BlockWriter<CompressedBlock>
    {
        /// <summary>Creates an instance of the CompressedBlockWriter class</summary>
        /// <returns cref="CompressedBlockWriter"></returns>
        /// <inheritdoc cref="File.OpenWrite"/>
        public static CompressedBlockWriter GetInstance(string filename)
        {
            var inputFile = File.OpenWrite(filename);
            return new CompressedBlockWriter(inputFile, false);
        }

        public CompressedBlockWriter(FileStream outputFile, bool leaveOpen) : base(outputFile, leaveOpen)
        { }

        protected override void WriteOutputBlock(BinaryWriter binaryWriter, CompressedBlock block)
        {
            binaryWriter.Write(block);
        }
    }
}
