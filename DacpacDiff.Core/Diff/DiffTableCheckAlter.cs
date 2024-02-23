using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Core.Diff
{
    public class DiffTableCheckAlter : IDifference
    {
        public TableCheckModel TargetTableCheck { get; }
        public TableCheckModel CurrentTableCheck { get; }

        public IModel Model => TargetTableCheck;
        public string Title => "Alter check constraint";
        public string Name => $"{TargetTableCheck.Table.FullName}.{(TargetTableCheck.IsSystemNamed ? "*" : $"[{TargetTableCheck.Name}]")}";

        public DiffTableCheckAlter(TableCheckModel targetTableCheck, TableCheckModel currentTableCheck)
        {
            TargetTableCheck = targetTableCheck ?? throw new ArgumentNullException(nameof(targetTableCheck));
            CurrentTableCheck = currentTableCheck ?? throw new ArgumentNullException(nameof(currentTableCheck));
        }
    }
}
