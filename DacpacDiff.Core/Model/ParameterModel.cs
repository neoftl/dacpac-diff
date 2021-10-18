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
            return this.IsEqual(other,
                m => m.FullName,
                m => m.Type,
                m => m.DefaultValue?.ScrubSQL(),
                m => m.Order,
                m => m.IsReadOnly,
                m => m.IsOutput);
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
    }
}
