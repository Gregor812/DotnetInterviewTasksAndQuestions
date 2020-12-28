﻿using GzipMT.DataStructures;
using System;
using System.IO;

namespace GzipMT.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static CompressedBlock ReadCompressedBlock(this BinaryReader binaryReader)
        {
            if (binaryReader == null)
                throw new ArgumentNullException(nameof(binaryReader));

            try
            {
                var blockNumber = binaryReader.ReadInt32();
                var blockSize = binaryReader.ReadInt32();
                var blockData = binaryReader.ReadBytes(blockSize);

                return new CompressedBlock
                {
                    Number = blockNumber,
                    Data = blockData
                };
            }
            catch (EndOfStreamException)
            {
                return default;
            }
        }
    }
}
