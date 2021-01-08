using GzipMT.Abstractions;
using GzipMT.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GzipMT.Application.FileBlockWorkers
{
    public abstract class BlockReader<T> : IBlockReader<T>
        where T : Block
    {
        private readonly FileStream _fileToRead;

        private int _blocksRead;

        public IQueue<T>[] Queues { get; }
        public ManualResetEventSlim ReadingDone { get; }

        protected abstract bool TryReadInputBlock(BinaryReader binaryReader, out T block);

        protected BlockReader(FileStream fileToRead, IQueue<T>[] queues)
        {
            _fileToRead = fileToRead;
            Queues = queues;
            ReadingDone = new ManualResetEventSlim();
        }

        public void Start(CancellationToken ct)
        {
            var t = new Thread(() => Read(ct));
            t.Start();
        }

        public void Read(CancellationToken ct)
        {
            var spinner = new SpinWait();
            foreach (var block in GetFileBlocks(ct))
            {
                var nextQueue = Queues[_blocksRead % Queues.Length];
                while (!(nextQueue.TryEnqueue(block) || ct.IsCancellationRequested))
                {
                    spinner.SpinOnce();
                }
                ++_blocksRead;
            }
            ReadingDone.Set();
        }

        private IEnumerable<T> GetFileBlocks(CancellationToken ct)
        {
            using (var binaryReader = new BinaryReader(_fileToRead))
            {
                while (!ct.IsCancellationRequested)
                {
                    if (TryReadInputBlock(binaryReader, out var block) && !ct.IsCancellationRequested)
                    {
                        yield return block;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileToRead?.Dispose();
                ReadingDone?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
