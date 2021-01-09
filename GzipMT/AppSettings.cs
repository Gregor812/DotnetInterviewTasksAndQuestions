using System;

namespace GzipMT
{
    public class AppSettings
    {
        public const int DefaultBufferSizeBytes = 1024 * 1024;

        public static readonly int MaxWorkerThreadsNumber = Math.Min(Environment.ProcessorCount, 8);

        public int? WorkerThreadsNumber { get; set; }
        public int? BufferSizeBytes { get; set; }

        public static AppSettings Default => new AppSettings
        {
            BufferSizeBytes = DefaultBufferSizeBytes,
            WorkerThreadsNumber = MaxWorkerThreadsNumber
        };
    }
}
