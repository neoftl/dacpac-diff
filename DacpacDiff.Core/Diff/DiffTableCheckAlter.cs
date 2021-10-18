using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Core.Diff
{
    public class DiffTableCheckAlter : IDifference
    {
        public TableCheckModel LeftTableCheck { get; }
        public TableCheckModel RightTableCheck { get; }

        public IModel Model => LeftTableCheck;
        public string Title => "Alter check constraint";
        public string Name => $"{LeftTableCheck.Table.FullName}.{(LeftTableCheck.IsSystemNamed ? "*" : $"[{LeftTableCheck.Name}]")}";

        public DiffTableCheckAlter(TableCheckModel leftTableCheck, TableCheckModel rightTableCheck)
        {
            LeftTableCheck = leftTableCheck ?? throw new ArgumentNullException(nameof(leftTableCheck));
            RightTableCheck = rightTableCheck ?? throw new ArgumentNullException(nameof(rightTableCheck));
        }
    }
}
