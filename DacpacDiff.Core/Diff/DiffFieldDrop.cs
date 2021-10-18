using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Core.Diff
{
    public class DiffFieldDrop : IDifference, IDataLossChange
    {
        public const string TITLE = "Drop table field";

        public FieldModel Field { get; }

        public IModel Model => Field;
        public string Name => $"{Field.Table.FullName}.[{Field.Name}]";
        public string Title => TITLE;

        public DiffFieldDrop(FieldModel field)
        {
            Field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public bool GetDataLossTable(out string tableName)
        {
            tableName = Field.Table.FullName;
            return true;
        }
    }
}
