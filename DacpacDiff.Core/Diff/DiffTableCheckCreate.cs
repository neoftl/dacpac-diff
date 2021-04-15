using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff
{
    public class DiffTableCheckCreate : IDifference
    {
        public TableCheckModel TableCheck { get; }

        public IModel Model => TableCheck;
        public string Title => "Create check constraint";
        public string Name => $"[{TableCheck.Table.Schema.Name}].[{TableCheck.Table.Name}].[{TableCheck.Name}]";

        public DiffTableCheckCreate(TableCheckModel tableCheck)
        {
            TableCheck = tableCheck;
        }
    }
}
