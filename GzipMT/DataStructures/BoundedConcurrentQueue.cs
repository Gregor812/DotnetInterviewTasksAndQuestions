using GzipMT.Abstractions;
using System.Collections.Generic;
using System.Threading;

namespace GzipMT.DataStructures
{
    public class BoundedConcurrentQueue<T> : IQueue<T>
    {
        private readonly int _maxItems;
        private readonly Queue<T> _queue;

        private int _nonLockExchange;

        public BoundedConcurrentQueue(int maxItems)
        {
            _maxItems = maxItems;
            _queue = new Queue<T>(_maxItems);
        }

        public bool IsEmpty => _queue.Count < 1;

        public bool TryEnqueue(T item)
        {
            if (Interlocked.Exchange(ref _nonLockExchange, 1) == 0)
            {
                try
                {
                    if (_queue.Count < _maxItems)
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

            item = default;
            return false;
        }
    }
}
