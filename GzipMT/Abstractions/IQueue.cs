namespace GzipMT.Abstractions
{
    public interface IQueue<T>
    {
        bool IsEmpty { get; }

        bool TryEnqueue(T item);

        bool TryDequeue(out T item);
    }
}
