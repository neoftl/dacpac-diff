namespace DacpacDiff.Core.Model
{
    public class TemporalityModel : IModel
    {
        public static readonly TemporalityModel Empty = new();

        public string Name { get; set; } = string.Empty;
        public string? PeriodFieldFrom { get; set; }
        public string? HistoryTable { get; set; }
        public string? PeriodFieldTo { get; set; }
    }
}
