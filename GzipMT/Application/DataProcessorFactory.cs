using GzipMT.Abstractions;
using GzipMT.Application.Dispatching;
using GzipMT.Application.FileBlockWorkers;
using GzipMT.Cli;
using GzipMT.DataStructures;
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
                    return CreateCompressor(bufferSizeBytes, workerThreadsNumber, o);
                case DecompressingOptions o:
                    return CreateDecompressor(bufferSizeBytes, workerThreadsNumber, o);
                default:
                    throw new ArgumentException($"Unknown options type: {options.GetType().Name}");
            }
        }

        private static IDataProcessor CreateDecompressor(int bufferSizeBytes, int workerThreadsNumber, DecompressingOptions o)
        {
            var inputQueues = new BoundedConcurrentQueue<CompressedBlock>[workerThreadsNumber];
            var outputQueues = new BoundedConcurrentQueue<UncompressedBlock>[workerThreadsNumber];
            for (int i = 0; i < workerThreadsNumber; ++i)
            {
                inputQueues[i] = new BoundedConcurrentQueue<CompressedBlock>(2);
                outputQueues[i] = new BoundedConcurrentQueue<UncompressedBlock>(2);
            }

            var compressedReader = CompressedBlockReader.GetInstance(o.InputFile, inputQueues);
            var uncompressedWriter = UncompressedBlockWriter.GetInstance(o.OutputFile, outputQueues);

            return new Decompressor(compressedReader, uncompressedWriter, bufferSizeBytes, workerThreadsNumber);
        }

        private static IDataProcessor CreateCompressor(int bufferSizeBytes, int workerThreadsNumber, CompressingOptions o)
        {
            var inputQueues = new BoundedConcurrentQueue<UncompressedBlock>[workerThreadsNumber];
            var outputQueues = new BoundedConcurrentQueue<CompressedBlock>[workerThreadsNumber];
            for (int i = 0; i < workerThreadsNumber; ++i)
            {
                inputQueues[i] = new BoundedConcurrentQueue<UncompressedBlock>(2);
                outputQueues[i] = new BoundedConcurrentQueue<CompressedBlock>(2);
            }

            var uncompressedReader = UncompressedBlockReader.GetInstance(o.InputFile, inputQueues, bufferSizeBytes);
            var compressedWriter = CompressedBlockWriter.GetInstance(o.OutputFile, outputQueues);

            return new Compressor(uncompressedReader, compressedWriter, workerThreadsNumber);
        }
    }
}
