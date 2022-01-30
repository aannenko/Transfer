namespace Transfer.App.Serialization;

public record TransferDto
{
    public (string Source, string Destination)[] Files { get; init; } = Array.Empty<ValueTuple<string, string>>();

    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Proxy { get; init; } = string.Empty;
}
