namespace GzipMT.Abstractions
{
    public interface IQueuedWorker<in TInput, TOutput>
    {
        bool TryEnqueue(TInput item);
        bool TryDequeue(out TOutput item);
    }
}
