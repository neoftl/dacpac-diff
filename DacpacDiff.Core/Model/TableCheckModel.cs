using DacpacDiff.Core.Utility;
using System;

namespace DacpacDiff.Core.Model
{
    public class TableCheckModel : IModel<TableCheckModel, TableModel>, IDependentModel
    {
        public TableModel Table { get; }
        public string Name { get; }
        public string FullName => $"[{Table.Schema.Name}].[{(IsSystemNamed ? "*" : Name)}]";
        public bool IsSystemNamed { get; }
        public string Definition { get; }
        public string[] Dependencies { get; set; } = Array.Empty<string>();

        public TableCheckModel(TableModel table, string? name, string definition)
        {
            Table = table;
            Name = name ?? string.Empty;
            IsSystemNamed = Name.Length == 0;
            Definition = definition.ReduceBrackets();
        }
    }
}
