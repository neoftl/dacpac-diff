namespace DacpacDiff.Core.Model
{
    public class TemporalityModel : IModel
    {
        public string Name { get; set; }
        public string? PeriodFieldFrom { get; set; }
        public string? HistoryTable { get; set; }
        public string? PeriodFieldTo { get; set; }
    }
}
