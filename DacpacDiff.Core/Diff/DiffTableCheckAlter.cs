using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff
{
    public class DiffTableCheckAlter : IDifference
    {
        public TableCheckModel LeftTableCheck { get; }
        public TableCheckModel RightTableCheck { get; }

        public IModel Model => LeftTableCheck ?? RightTableCheck;
        public string Title => "Alter check constraint";
        public string Name => $"[{LeftTableCheck.Table.Schema.Name}].[{LeftTableCheck.Table.Name}].[{LeftTableCheck.Name}]";

        public DiffTableCheckAlter(TableCheckModel leftTableCheck, TableCheckModel rightTableCheck)
        {
            LeftTableCheck = leftTableCheck;
            RightTableCheck = rightTableCheck;
        }

        public override string ToString()
        {
            return $"ALTER TABLE [{RightTableCheck.Table.Schema.Name}].[{RightTableCheck.Table.Name}] DROP CONSTRAINT [{RightTableCheck.Name}]\r\n"
                + "GO\r\n"
                + $"ALTER TABLE [{LeftTableCheck.Table.Schema.Name}].[{LeftTableCheck.Table.Name}] ADD CONSTRAINT [{LeftTableCheck.Name}] CHECK {LeftTableCheck.Definition}";
        }
    }
}
