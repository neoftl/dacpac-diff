using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Core.Diff
{
    public class DiffFieldCreate : IDifference
    {
        public const string TITLE = "Add table field";

        public FieldModel Field { get; }

        public IModel Model => Field;
        public string Name => $"{Field.Table.FullName}.[{Field.Name}]";
        public string Title => TITLE;

        public DiffFieldCreate(FieldModel field)
        {
            Field = field ?? throw new ArgumentNullException(nameof(field));
        }
    }
}
