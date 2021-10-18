using DacpacDiff.Core.Model;
using System;

namespace DacpacDiff.Core.Diff
{
    // TODO: should be individual diff per type of alter?
    public class DiffFieldAlter : IDifference, IDataLossChange
    {
        public const string TITLE = "Alter table field";

        public FieldModel LeftField { get; }
        public FieldModel RightField { get; }

        public IModel Model => LeftField;
        public string Name => $"{LeftField.Table.FullName}.[{LeftField.Name}]";
        public string Title => TITLE;

        public DiffFieldAlter(FieldModel lft, FieldModel rgt)
        {
            LeftField = lft ?? throw new ArgumentNullException(nameof(lft));
            RightField = rgt ?? throw new ArgumentNullException(nameof(rgt));
        }

        public bool GetDataLossTable(out string tableName)
        {
            // TODO: More accurate test
            // numeric precision: decimal(x,y) = (x-y).y = if lft > rgt, dataloss
            tableName = RightField.Table.FullName;
            return LeftField.Type != RightField.Type;
        }
    }
}
