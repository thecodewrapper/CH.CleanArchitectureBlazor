namespace CH.CleanArchitecture.Core.Application
{
    public class ClaimData
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public ClaimData() { }
        public ClaimData(string type, string value) {
            Type = type;
            Value = value;
        }
    }
}
