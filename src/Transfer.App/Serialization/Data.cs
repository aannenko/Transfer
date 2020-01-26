namespace Transfer.App.Serialization
{
    public class Data
    {
        public (string Source, string Destination)[] Files { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Proxy { get; set; }
    }
}