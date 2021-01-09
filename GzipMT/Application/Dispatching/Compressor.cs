using GzipMT.Abstractions;
using GzipMT.Application.GZip;
using GzipMT.DataStructures;

namespace GzipMT.Application.Dispatching
{
    public class Compressor :
        DataProcessor<UncompressedBlock, CompressedBlock, CompressionWorker>
    {
        public Compressor(IBlockReader<UncompressedBlock> reader, IBlockWriter<CompressedBlock> writer,
            int workerThreadsNumber)
            : base(reader, writer, workerThreadsNumber)
        { }

        protected override CompressionWorker CreateWorker(IQueue<UncompressedBlock> inputQueue, IQueue<CompressedBlock> outputQueue)
        {
            return new CompressionWorker(inputQueue, outputQueue);
        }
    }
}
