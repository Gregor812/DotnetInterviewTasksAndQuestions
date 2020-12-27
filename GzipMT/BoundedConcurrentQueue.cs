using System.Collections.Generic;
using System.Threading;

namespace GzipMT
{
    class BoundedConcurrentQueue<T>
    {
        private const int MaxItems = 8;

        private readonly Queue<T> _queue = new Queue<T>(MaxItems);

        private int _nonLockExchange;

        public bool IsEmpty => _queue.Count < 1;

        public bool TryEnqueue(T item)
        {
            if (Interlocked.Exchange(ref _nonLockExchange, 1) == 0)
            {
                try
                {
                    if (_queue.Count < MaxItems)
                    {
                        _queue.Enqueue(item);
                        return true;
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _nonLockExchange, 0);
                }
            }

            return false;
        }

        public bool TryDequeue(out T item)
        {
            if (Interlocked.Exchange(ref _nonLockExchange, 1) == 0)
            {
                try
                {
                    if (_queue.Count > 0)
                    {
                        item = _queue.Dequeue();
                        return true;
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _nonLockExchange, 0);
                }
            }

            item = default(T);
            return false;
        }
    }
}
