using System.Collections.Concurrent;

namespace Transfer.Core;

public class ReaderWriterRegistry
{
    private readonly ConcurrentStack<(Func<object, bool> selector, Func<object, IReader> factory)> _readers =
        new ConcurrentStack<(Func<object, bool>, Func<object, IReader>)>();

    private readonly ConcurrentStack<(Func<object, bool> selector, Func<object, IWriter> factory)> _writers =
        new ConcurrentStack<(Func<object, bool>, Func<object, IWriter>)>();

    public void RegisterReader<T>(Func<T, bool> selector, Func<T, IReader> factory)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        _readers.Push((
            t => t is T tConverted && selector(tConverted),
            t => factory((T)t)));
    }

    public void RegisterWriter<T>(Func<T, bool> selector, Func<T, IWriter> factory)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        if (factory == null) throw new ArgumentNullException(nameof(factory));

        _writers.Push((
            t => t is T tConverted && selector(tConverted),
            t => factory((T)t)));
    }

    public TransferInfo GetTransferInfo<T, K>(T source, K destination, string? description = null,
        IProgress<double>? progress = null, CancellationToken token = default)
        where T : notnull
        where K : notnull
    {
        var reader = _readers.FirstOrDefault(r => r.selector is Func<T, bool> condition && condition(source)).factory(source);
        if (reader == null) throw new InvalidOperationException($"Cannot find suitable reader for the source '{source}'");

        var writer = _writers.FirstOrDefault(w => w.selector is Func<K, bool> condition && condition(destination)).factory(destination);
        if (writer == null) throw new InvalidOperationException($"Cannot find suitable writer for the destination '{destination}'");

        return new TransferInfo(reader, writer, description, progress, token);
    }
}
