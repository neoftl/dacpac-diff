using DacpacDiff.Core.Utility;
using System;

namespace DacpacDiff.Core.Model
{
    public class FieldRefModel : IModel<FieldRefModel, FieldModel>, IEquatable<FieldRefModel>
    {
        /// <summary>
        /// The field that defines this reference.
        /// </summary>
        public FieldModel Field { get; }
        public TableModel Table => Field.Table;

        /// <summary>
        /// The field that this reference targets.
        /// </summary>
        public FieldModel TargetField { get; }

        public string FullName => $"{Field.FullName}:[{Name}]";
        /// <summary>
        /// The name of this reference, if any.
        /// </summary>
        public string Name { get; init; }
        /// <summary>
        /// True if the reference is system named.
        /// </summary>
        public bool IsSystemNamed { get; init; }

        public FieldRefModel(FieldRefModel fref)
        {
            Name = fref.Name;
            IsSystemNamed = fref.IsSystemNamed;
            Field = fref.Field;
            TargetField = fref.TargetField;
        }
        public FieldRefModel(FieldModel field, FieldModel target)
        {
            Name = string.Empty;
            IsSystemNamed = true;
            Field = field;
            TargetField = target;
        }

        public bool Equals(FieldRefModel? rgt)
        {
            return Field.FullName == rgt?.Field.FullName
                && TargetField.FullName == rgt.TargetField.FullName
                && (IsSystemNamed ? rgt.IsSystemNamed : Name == rgt.Name);
        }
        public override bool Equals(object? obj) => Equals(obj as FieldRefModel);

        public override int GetHashCode()
        {
            return new object[]
            {
                Field,
                TargetField,
                Name,
                IsSystemNamed
            }.CalculateHashCode();
        }
    }
}
