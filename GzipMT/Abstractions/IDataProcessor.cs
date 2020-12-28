using System.Threading;

namespace GzipMT.Abstractions
{
    public interface IDataProcessor
    {
        int Run(CancellationToken ct);
        void WaitForExit();
    }
}
