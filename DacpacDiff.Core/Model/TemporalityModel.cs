using DacpacDiff.Core.Utility;
using System;

namespace DacpacDiff.Core.Model
{
    public class TemporalityModel : IModel, IEquatable<TemporalityModel>
    {
        public static readonly TemporalityModel Empty = new();

        public string Name { get; set; } = string.Empty;
        public string? PeriodFieldFrom { get; set; }
        public string? PeriodFieldTo { get; set; }
        public string? HistoryTable { get; set; }

        public bool Equals(TemporalityModel? other)
        {
            return this.IsEqual(other,
                m => m.Name,
                m => m.PeriodFieldFrom,
                m => m.PeriodFieldTo,
                m => m.HistoryTable);
        }
        public override bool Equals(object? obj) => Equals(obj as TemporalityModel);

        public override int GetHashCode()
        {
            return new object?[]
            {
                Name,
                PeriodFieldFrom,
                PeriodFieldTo,
                HistoryTable,
            }.CalculateHashCode();
        }
    }
}
