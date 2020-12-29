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
                    return new Compressor(o, bufferSize,
                        new UncompressedBlockReader(bufferSize));
                case DecompressingOptions o:
                    return new Decompressor(o, bufferSize,
                        new CompressedBlockReader(bufferSize));
                default:
                    throw new ArgumentException($"Unknown options type: {options.GetType().Name}");
            }
        }
    }
}
