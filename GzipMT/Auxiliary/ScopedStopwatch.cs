using System;
using System.Diagnostics;

namespace GzipMT.Auxiliary
{
    public class ScopedStopwatch : IDisposable
    {
        private readonly Stopwatch _stopwatch;

        public ScopedStopwatch()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
        }

        public TimeSpan Elapsed => _stopwatch.Elapsed;
    }
}
