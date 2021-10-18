using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Core.Diff
{
    public class DiffTableCheckDrop : IDifference
    {
        public TableCheckModel TableCheck { get; }

        public IModel Model => TableCheck;
        public string Title => "Drop check constraint";
        public string Name => $"{TableCheck.Table.FullName}.{(TableCheck.IsSystemNamed ? "*" : $"[{TableCheck.Name}]")}";

        public DiffTableCheckDrop(TableCheckModel tableCheck)
        {
            TableCheck = tableCheck ?? throw new ArgumentNullException(nameof(tableCheck));
        }
    }
}
