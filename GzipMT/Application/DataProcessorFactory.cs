using GzipMT.Abstractions;
using GzipMT.Cli;
using System;

namespace GzipMT.Application
{
    public class DataProcessorFactory
    {
        public static IDataProcessor GetInstance(ProcessingOptions options,
            int bufferSize)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            switch (options)
            {
                case CompressingOptions o:
                    var uncompressedReader =
                        UncompressedBlockReader.GetInstance(o.InputFile, bufferSize);
                    return new Compressor(o, bufferSize, uncompressedReader);
                case DecompressingOptions o:
                    var compressedReader =
                        CompressedBlockReader.GetInstance(o.InputFile, bufferSize);
                    return new Decompressor(o, bufferSize, compressedReader);
                default:
                    throw new ArgumentException($"Unknown options type: {options.GetType().Name}");
            }
        }
    }
}
