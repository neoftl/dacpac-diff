namespace DacpacDiff.Core.Model
{
    public class TableCheckModel : IModel<TableCheckModel, TableModel>
    {
        public TableModel Table { get; }
        public string Name { get; }
        public bool IsSystemNamed { get; }
        public string Definition { get; }

        public TableCheckModel(TableModel table, string? name, string definition)
        {
            Table = table;
            Name = name ?? string.Empty;
            IsSystemNamed = Name.Length == 0;
            Definition = definition;

            while (Definition.Length > 2 && Definition[0] == '(' && Definition[^1] == ')')
            {
                Definition = Definition[1..^1];
            }
        }
    }
}
