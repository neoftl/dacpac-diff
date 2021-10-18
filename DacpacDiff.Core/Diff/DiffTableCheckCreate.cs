using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Core.Diff
{
    public class DiffTableCheckCreate : IDifference
    {
        public TableCheckModel TableCheck { get; }

        public IModel Model => TableCheck;
        public string Title => "Create check constraint";
        public string Name => $"{TableCheck.Table.FullName}.{(TableCheck.IsSystemNamed ? "*" : $"[{TableCheck.Name}]")}";

        public DiffTableCheckCreate(TableCheckModel tableCheck)
        {
            TableCheck = tableCheck ?? throw new ArgumentNullException(nameof(tableCheck));
        }
    }
}
