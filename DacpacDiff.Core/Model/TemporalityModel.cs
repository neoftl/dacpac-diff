using DacpacDiff.Core.Utility;

namespace DacpacDiff.Core.Model;

public class TemporalityModel : IModel<TemporalityModel, TableModel>, IEquatable<TemporalityModel>
{
    public static readonly TemporalityModel Empty = new(TableModel.Empty);

    public TableModel Table { get; }
    public string Name => $"{Table.FullName} temporality";
    public string FullName => Name;
    public string? PeriodFieldFrom { get; set; }
    public string? PeriodFieldTo { get; set; }
    public string? HistoryTable { get; set; }

    public TemporalityModel(TableModel table)
    {
        Table = table;
    }

    public bool Equals(TemporalityModel? other)
    {
        return this.IsEqual(other,
            m => m.Table.FullName,
            m => m.PeriodFieldFrom,
            m => m.PeriodFieldTo,
            m => m.HistoryTable);
    }
    public override bool Equals(object? obj) => Equals(obj as TemporalityModel);

    public override int GetHashCode()
    {
        return new object?[]
        {
            Table.FullName,
            PeriodFieldFrom,
            PeriodFieldTo,
            HistoryTable,
        }.CalculateHashCode();
    }
}
