//using System.IO;
//using System.Threading;
//using GzipMT.Abstractions;
//using GzipMT.DataStructures;

//namespace GzipMT.Application
//{
//    public abstract class BlockWriter<T> : IBlockWriter<T>
//        where T : Block
//    {
//        public int BlocksWritten { get; private set; } // TODO: what if file is already written

//        protected int BufferSize;

//        protected BlockWriter(int bufferSize)
//        {
//            BufferSize = bufferSize;
//        }

//        public void WriteFileBlock(T block, string filename, CancellationToken ct)
//        {
//            using (var outputFile = File.OpenWrite(outputFileName))
//            using (var binaryWriter = new BinaryWriter(outputFile))
//            {
//                while (!(ProcessingDone.IsSet && OutputQueue.IsEmpty || ct.IsCancellationRequested))
//                {
//                    if (OutputQueue.TryDequeue(out var block))
//                    {
//                        WriteOutputBlock(binaryWriter, block);
//                        ++BlocksWritten;
//                    }
//                    else
//                    {
//                        spinner.SpinOnce();
//                    }
//                }
//            }
//        }
//        protected abstract void WriteOutputBlock(BinaryWriter binaryWriter, T block);
//    }
//}
