using DacpacDiff.Core.Utility;
using System;

namespace DacpacDiff.Core.Model
{
    public class ParameterModel : IModel<ParameterModel, IParameterisedModuleModel>, IEquatable<ParameterModel>
    {
        public IParameterisedModuleModel? Parent { get; }
        public string Name { get; }
        public string FullName => $"{Parent?.FullName}.[{Name}]";
        public string? Type { get; set; }

        public bool HasDefault => DefaultValue != null;
        public string? DefaultValue { get; set; }

        public int Order { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsOutput { get; set; }

        public ParameterModel(IParameterisedModuleModel parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        public bool Equals(ParameterModel? other)
        {
            if (other is null)
            {
                return false;
            }

            bool eq<T>(Func<ParameterModel, T?> fn) where T : IEquatable<T>
            {
                var l = fn(this);
                var r = fn(other);
                return (l is null && r is null) || l?.Equals(r) == true;
            }
            return eq(m => m.FullName)
                && eq(m => m.Type)
                && IsDefaultMatch(other)
                && eq(m => m.Order)
                && eq(m => m.IsReadOnly)
                && eq(m => m.IsOutput);
        }
        public override bool Equals(object? obj) => Equals(obj as ParameterModel);

        public override int GetHashCode()
        {
            return new object?[]
            {
                Parent,
                Name,
                Type,
                DefaultValue,
                Order,
                IsReadOnly,
                IsOutput
            }.CalculateHashCode();
        }

        public bool IsDefaultMatch(ParameterModel field)
        {
            if (DefaultValue != null)
            {
                if (field.DefaultValue == null)
                {
                    return false;
                }

                var dbL = DefaultValue.ScrubSQL();
                var dbR = field.DefaultValue.ScrubSQL();
                return dbL == dbR;
            }

            return !field.HasDefault;
        }
    }
}
