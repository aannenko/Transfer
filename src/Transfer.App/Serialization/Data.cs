namespace Transfer.App.Serialization
{
    public record Data
    {
        public (string Source, string Destination)[] Files { get; init; }

        public string UserName { get; init; }

        public string Password { get; init; }

        public string Proxy { get; init; }
    }
}