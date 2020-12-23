namespace ConsoleApp
{
    public class Rule
    {
        public string When { get; set; }
        public string Value { get; set; }

        public bool IsDefault => string.IsNullOrEmpty(When);
    }
}