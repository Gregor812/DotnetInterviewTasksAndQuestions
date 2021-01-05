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
                    var compressedWriter =
                        CompressedBlockWriter.GetInstance(o.OutputFile);
                    return new Compressor(o.WorkerThreadsNumber, uncompressedReader, compressedWriter);
                case DecompressingOptions o:
                    var compressedReader =
                        CompressedBlockReader.GetInstance(o.InputFile);
                    var uncompressedWriter =
                        UncompressedBlockWriter.GetInstance(o.OutputFile);
                    return new Decompressor(o.WorkerThreadsNumber, compressedReader, uncompressedWriter, bufferSize);
                default:
                    throw new ArgumentException($"Unknown options type: {options.GetType().Name}");
            }
        }
    }
}
