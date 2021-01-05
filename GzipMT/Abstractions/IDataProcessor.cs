using System;
using System.Threading;

namespace GzipMT.Abstractions
{
    public interface IDataProcessor : IDisposable
    {
        int Run(CancellationToken ct);
        void WaitForExit();
    }
}
