using GzipMT.Abstractions;
using GzipMT.Cli;
using System;

namespace GzipMT.Application
{
    public class DataProcessorFactory
    {
        public static IDataProcessor GetInstance(ProcessingOptions options,
            int bufferSizeBytes, int workerThreadsNumber)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            switch (options)
            {
                case CompressingOptions o:
                    var uncompressedReader =
                        UncompressedBlockReader.GetInstance(o.InputFile, bufferSizeBytes);
                    var compressedWriter =
                        CompressedBlockWriter.GetInstance(o.OutputFile);
                    return new Compressor(uncompressedReader, compressedWriter, workerThreadsNumber);
                case DecompressingOptions o:
                    var compressedReader =
                        CompressedBlockReader.GetInstance(o.InputFile);
                    var uncompressedWriter =
                        UncompressedBlockWriter.GetInstance(o.OutputFile);
                    return new Decompressor(compressedReader, uncompressedWriter, bufferSizeBytes, workerThreadsNumber);
                default:
                    throw new ArgumentException($"Unknown options type: {options.GetType().Name}");
            }
        }
    }
}
