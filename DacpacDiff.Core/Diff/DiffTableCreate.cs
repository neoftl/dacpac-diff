using DacpacDiff.Core.Model;

namespace DacpacDiff.Core.Diff
{
    public class DiffTableCreate : IDifference
    {
        public TableModel Table { get; }

        public IModel Model => Table;
        public string Title => "Create table";
        public string Name => Table.FullName;

        public DiffTableCreate(TableModel table)
        {
            Table = table;
        }
    }
}
