using GzipMT.DataStructures;
using System;
using System.IO;

namespace GzipMT.Extensions
{
    public static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter binaryWriter, CompressedBlock block)
        {
            if (binaryWriter == null)
                throw new ArgumentNullException(nameof(binaryWriter));

            binaryWriter.Write(block.Number);
            binaryWriter.Write(block.Data.Length);
            binaryWriter.Write(block.Data);
        }

        public static void Write(this BinaryWriter binaryWriter, UncompressedBlock block)
        {
            if (binaryWriter == null)
                throw new ArgumentNullException(nameof(binaryWriter));

            binaryWriter.Write(block.Data);
        }
    }
}
