namespace DacpacDiff.Core.Output
{
    public class SqlComment : ISqlFormattable
    {
        public string? Title => null;
        public string Name => string.Empty;

        public string Comment { get; set; } = string.Empty;
    }
}
