using GzipMT.Abstractions;
using GzipMT.Application.GZip;
using GzipMT.DataStructures;

namespace GzipMT.Application.Dispatching
{
    public class Decompressor :
        DataProcessor<CompressedBlock, UncompressedBlock, DecompressionWorker>
    {
        private readonly int _bufferSizeBytes;

        public Decompressor(IBlockReader<CompressedBlock> reader, IBlockWriter<UncompressedBlock> writer,
            int bufferSizeBytes, int workerThreadsNumber)
            : base(reader, writer, workerThreadsNumber)
        {
            _bufferSizeBytes = bufferSizeBytes;
        }

        protected override DecompressionWorker CreateWorker(IQueue<CompressedBlock> inputQueue, IQueue<UncompressedBlock> outputQueue)
        {
            return new DecompressionWorker(inputQueue, outputQueue, _bufferSizeBytes);
        }
    }
}
