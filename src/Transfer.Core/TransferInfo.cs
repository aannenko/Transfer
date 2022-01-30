namespace Transfer.Core;

public class TransferInfo
{
    internal TransferInfo(IReader reader, IWriter writer, string? description = null,
        IProgress<double>? progress = null, CancellationToken token = default)
    {
        Transfer = new Transfer(reader, writer);
        Description = description ?? string.Empty;
        Progress = progress;
        Token = token;
    }

    internal Transfer Transfer { get; }

    public string Description { get; }

    public IProgress<double>? Progress { get; }

    public CancellationToken Token { get; }
}
