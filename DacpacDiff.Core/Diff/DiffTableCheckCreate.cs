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

        public override string ToString()
        {
            if (TableCheck.IsSystemNamed)
            {
                return $"ALTER TABLE [{TableCheck.Table.Schema.Name}].[{TableCheck.Table.Name}] ADD CHECK {TableCheck.Definition}";
            }
            return $"ALTER TABLE [{TableCheck.Table.Schema.Name}].[{TableCheck.Table.Name}] ADD CONSTRAINT [{TableCheck.Name}] CHECK {TableCheck.Definition}";
        }
    }
}
