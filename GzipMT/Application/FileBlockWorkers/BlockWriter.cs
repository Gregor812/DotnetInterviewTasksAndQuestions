using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GzipMT.Application.FileBlockWorkers
{
    public abstract class BlockWriter<T> : IBlockWriter<T>
        where T : Block
    {
        private readonly FileStream _fileToWrite;

        private int _blocksWritten;

        public IQueue<T>[] Queues { get; }
        public ManualResetEventSlim WritingDone { get; }

        protected abstract void WriteOutputBlock(BinaryWriter binaryWriter, T block);

        protected BlockWriter(FileStream fileToWrite, IQueue<T>[] queues)
        {
            _fileToWrite = fileToWrite;
            Queues = queues;
            WritingDone = new ManualResetEventSlim();
        }

        public void Start(CountdownEvent processingDone, CancellationToken ct)
        {
            var t = new Thread(() => Write(processingDone, ct));
            t.Start();
        }

        private void Write(CountdownEvent processingDone, CancellationToken ct)
        {
            var spinner = new SpinWait();
            while (!(processingDone.IsSet && Queues.All(q => q.IsEmpty) || ct.IsCancellationRequested))
            {
                var nextQueue = Queues[_blocksWritten % Queues.Length];
                if (nextQueue.TryDequeue(out var block))
                {
                    WriteFileBlock(block);
                    ++_blocksWritten;
                }
                else
                {
                    spinner.SpinOnce();
                }
            }
            WritingDone.Set();
        }

        public void Dispose()
        {
            _fileToWrite?.Dispose();
            WritingDone?.Dispose();
        }

        private void WriteFileBlock(T block)
        {
            using (var binaryWriter = new BinaryWriter(_fileToWrite, Encoding.Default, true))
            {
                WriteOutputBlock(binaryWriter, block);
            }
        }
    }
}
