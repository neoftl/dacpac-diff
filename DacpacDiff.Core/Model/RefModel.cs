using System;

namespace DacpacDiff.Core.Model
{
    public class RefModel : IModel<RefModel, TableModel>, IEquatable<RefModel>
    {
        public static readonly RefModel Empty = new RefModel();

        public TableModel Table { get; private set; } = TableModel.Empty;
        public string Name { get; set; }
        public bool IsSystemNamed { get; }

        public string Field { get; }
        public string TargetTable { get; }
        public string TargetField { get; }

        private RefModel()
        {
            Name = string.Empty;
            Field = string.Empty;
            TargetTable = string.Empty;
            TargetField = string.Empty;
        }
        public RefModel(RefModel tref)
        {
            Table = tref.Table;
            Name = tref.Name;
            IsSystemNamed = tref.IsSystemNamed;
            Field = tref.Field;
            TargetField = tref.TargetField;
            TargetTable = tref.TargetTable;
        }
        public RefModel(FieldModel field)
        {
            Table = field.Table;
            Name = field.Ref?.Name ?? string.Empty;
            IsSystemNamed = field.Ref?.IsSystemNamed == true;
            Field = field.Name;
            TargetTable = field.Ref?.TargetTable ?? string.Empty;
            TargetField = field.Ref?.TargetField ?? string.Empty;
        }

        public bool Equals(RefModel? rgt)
        {
            return Field == rgt?.Field
                && Table.FullName == rgt.Table.FullName
                && TargetTable == rgt.TargetTable
                && TargetField == rgt.TargetField
                && (!IsSystemNamed ? !rgt.IsSystemNamed : Name == rgt.Name);
        }
        public override bool Equals(object? obj) => Equals(obj as RefModel);

        public override int GetHashCode()
        {
            // TODO
            return base.GetHashCode();
        }
    }
}
