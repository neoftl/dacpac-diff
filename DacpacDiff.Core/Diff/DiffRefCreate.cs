using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Core.Diff
{
    public class DiffRefCreate : IDifference
    {
        public FieldModel Field { get; }

        public IModel Model => Field;
        public string Title => "Create reference";
        public string Name => $"{Field.Table.FullName}.[{Field.Name}]:[{Field.RefName}]";

        public DiffRefCreate(FieldModel fld)
        {
            Field = fld ?? throw new ArgumentNullException(nameof(fld));
        }

        public override string ToString()
        {
            return new RefModel(Field).GetAddSql();
        }
    }
}
